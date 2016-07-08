/* Copyright © 2015-2016 Noesys Software Pvt.Ltd. - All Rights Reserved
 * -------------
 * This file is part of Infoveave.
 * Infoveave is dual licensed under Infoveave Commercial License and AGPL v3  
 * -------------
 * You should have received a copy of the GNU Affero General Public License v3
 * along with this program (Infoveave)
 * You can be released from the requirements of the license by purchasing
 * a commercial license. Buying such a license is mandatory as soon as you
 * develop commercial activities involving the Infoveave without
 * disclosing the source code of your own applications.
 * -------------
 * Authors: Naresh Jois <naresh@noesyssoftware.com>, et al.
 */
using Infoveave.Data.Interfaces;
using Infoveave.Helpers;
using Infoveave.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace Infoveave.Controllers
{
    /// <summary>
    /// Role Controller
    /// </summary>
    [Authorize]
    [Route("api/{version}/Role")]
    public class RoleController:BaseController
    {
        private readonly IHostingEnvironment _appEnvironment;

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        /// <param name="appEnvironment"></param>
        public RoleController(ITenantContext context, IOptions<ApplicationConfiguration> configuration, IHostingEnvironment appEnvironment) 
            : base(context,configuration:configuration)
        {
            _appEnvironment = appEnvironment;
        }


        /// <summary>
        /// Get Roles
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("")]
        [Versions("v2")]
        public async Task<List<Role>> GetRoles(CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var roles = await tenantContext.UserRoles.GetAll().ToListAsync(cancellationToken);
            return roles.Where(r => r.CreatedBy != 0).Select(r => new Role()
            {
                Id=r.Id,
                Name = r.Name,
                Permissions = r.Permissions,
                CreatedBy = (tenantContext.Users.GetAll().Where(u => u.Id == r.CreatedBy).FirstOrDefault() == null) ? "" : tenantContext.Users.GetAll().Where(u => u.Id == r.CreatedBy).FirstOrDefault().UserName,
                CreatedOn =r.CreatedOn
            }).ToList();          
        }


        /// <summary>
        /// Get Role by Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Versions("v2")]
        public async Task<Role> GetRole(long id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var role = await tenantContext.UserRoles.GetAll().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
            return  new Role()
            {
                Id = role.Id,
                Name = role.Name,
                Permissions = role.Permissions,
                CreatedBy =(role.CreatedBy ==0)? "": ( tenantContext.Users.GetAll().Where(u =>u.Id ==role.CreatedBy)== null) ? "" : tenantContext.Users.GetAll().Where(u => u.Id == role.CreatedBy).FirstOrDefault().UserName,
                CreatedOn = role.CreatedOn
            };
        }

        /// <summary>
        /// Create Role
        /// </summary>
        /// <param name="role"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpPost("")]
        public async Task<Role> AddRole([FromBody]Role role, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            if (role.Name == null)
            {
                throw new Exception("AUTH-R001");
            }
            var existingRole = await tenantContext.UserRoles.GetAll().Where(u => u.Name.ToLower() == role.Name.ToLower()).ToListAsync(cancellationToken);
            if (existingRole.FirstOrDefault() != null )
            {
                throw new Exception("AUTH-R002");
            }
            var newRole = new Models.UserRole
            {
                Name = role.Name,
                Permissions =role.Permissions,
                CreatedBy = CurrentUser.Id,
                CreatedOn = DateTime.Now
            };
            tenantContext.UserRoles.Add(newRole);
            await tenantContext.CommitAsync(cancellationToken);
            return new Role { Id = newRole.Id, Name = newRole.Name, Permissions = newRole.Permissions };
        }

        /// <summary>
        /// Update Role
        /// </summary>
        /// <param name="id"></param>
        /// <param name="role"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(long id, [FromBody]Role role, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var userRole = await tenantContext.UserRoles.GetAll().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
            if (userRole == null)
            {
                throw new Exception("AUTH-R003");
            }
            userRole.Name = role.Name;
            userRole.Permissions = role.Permissions;
            tenantContext.UserRoles.Update(userRole);
            await tenantContext.CommitAsync();
            return Ok();
        }

        /// <summary>
        /// Delete Role
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(long id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var role = await tenantContext.UserRoles.GetAll().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
            if (role == null)
            {
                throw new Exception("AUTH-R003");
            }
            var users = await tenantContext.Users.GetAll().Where(u => u.UserRoleId == id).ToListAsync(cancellationToken);
            if (users.Count >0)
            {
                throw new Exception("AUTH-R004");
            }
            tenantContext.UserRoles.Delete(role);
            await tenantContext.CommitAsync();
            return Ok();
        }

        /// <summary>
        /// Get available Permissions
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("Permissions")]
        [Versions("v2")]
        public List<Permission> GetPermissions(CancellationToken cancellationToken = default(CancellationToken))
        {
            
            var permissions = new List<Permission>();
            permissions = JsonConvert.DeserializeObject<List<Permission>>(System.IO.File.ReadAllText(System.IO.Path.Combine(_appEnvironment.ContentRootPath,"permission.json")));
            return permissions;   
        }

    }
}
