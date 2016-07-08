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
using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;

namespace Infoveave.Controllers
{
    /// <summary>
    /// User Controller
    /// </summary>
    [Authorize]
    [Route("api/{version}/User")]
    public class UserController : BaseController
    {
        private readonly IHostingEnvironment _appEnvironment;
        private Mailer.IMailer Mailer { get; set; }


        /// <summary>
        /// Constuctor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        /// <param name="mailer"></param>
        /// <param name="appEnvironment"></param>
        /// <param name="cacheProvider"></param>
        public UserController(ITenantContext context, IOptions<ApplicationConfiguration> configuration, Mailer.IMailer mailer, IHostingEnvironment appEnvironment, CacheProvider.ICacheProvider cacheProvider)
            : base(context, configuration: configuration, cacheProvider: cacheProvider)
        {
            _appEnvironment = appEnvironment;
            Mailer = mailer;
        }

        /// <summary>
        /// Get All Users
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        ///
        [HttpGet("")]
        [Versions("v2")]
        public async Task<List<User>> GetUsers(CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var users = await tenantContext.Users.GetAll().Where(u => u.Id != CurrentUser.Id).ToListAsync(cancellationToken);
            return users.Select(u => new User()
            {
                Id = u.Id,
                UserName = u.UserName,
                FirstName = u.FirstName,
                LastName = u.LastName,
                CreatedOn = u.CreatedOn,
                IsLocked = u.IsLockedOut,
                Email = u.Email,
                RoleId = u.UserRoleId
            }).ToList();
        }

        /// <summary>
        /// Get User by Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Versions("v2")]
        public async Task<User> GetUser(long id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var user = await tenantContext.Users.GetAll().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
            return new User()
            {
                Id = user.Id,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                CreatedOn = user.CreatedOn,
                RoleId = user.UserRoleId,
                UserContext = user.Context
            };
        }

        /// <summary>
        /// Create User
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpPost("")]
        public async Task<User> AddUser([FromBody]User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            if (user.UserName == null)
            {
                throw new Exception("AUTH-U001");
            }
            var existingUser = await tenantContext.Users.GetAll().Where(u => u.UserName.ToLower() == user.UserName.ToLower()).ToListAsync(cancellationToken);
            if (existingUser.FirstOrDefault() != null)
            {
                throw new Exception("AUTH-U002");
            }
            var newUser = new Models.User
            {
                UserName = user.UserName.ToLower(),
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email.ToLower(),
                CreatedOn = DateTime.Now,
                IsArchived = false,
                IsLockedOut = false,
                LoginAttempts = 0,
                Language = "en",
                UserRoleId = user.RoleId,
                ImagePath = user.ImagePath,
                Context = user.UserContext
            };

            var password = (!string.IsNullOrEmpty(user.NewPassword)) ? user.NewPassword : RandomString();
            newUser.SetPassword(password);
            tenantContext.Users.Add(newUser);
            await tenantContext.CommitAsync(cancellationToken);
            try
            {
                if (user.ImportDashboards.HasValue && user.ImportDashboards.Value)
                {
                    var file = Path.Combine(Configuration.Application.Paths["Reports"], CurrentUser.Tenant, "defaultInfoboards.json");
                    if (System.IO.File.Exists(file))
                    {
                        var content = System.IO.File.ReadAllText(file);

                        var infoboards = JsonConvert.DeserializeObject<List<Models.Infoboard>>(content);
                        // If here we know we can proceed
                        var existingInfoboards = await tenantContext.Infoboards.GetAll().Where(i => i.CreatedBy == CurrentUser.Id).ToListAsync();
                        existingInfoboards.ForEach(ei => tenantContext.Infoboards.Delete(ei));
                        await tenantContext.CommitAsync();

                        var newInfoboards = infoboards.Select(i =>
                        {
                            return new Models.Infoboard()
                            {
                                Layouts = i.Layouts,
                                Name = i.Name,
                                InfoboardOptions = i.InfoboardOptions,
                                CreatedBy = newUser.Id,
                                CreatedOn = DateTime.Now,
                                IsPublic = false,
                                ShortCode = null,
                                SortOrder = i.SortOrder,
                                Items = i.Items.Select(il =>
                                {
                                    return new Models.InfoboardLink
                                    {
                                        HorizontalSize = il.HorizontalSize,
                                        VerticalSize = il.VerticalSize,
                                        WidgetId = il.WidgetId,
                                        PositionX = il.PositionX,
                                        PositionY = il.PositionY,
                                        SavedViewShortCode = il.SavedViewShortCode,
                                    };
                                }).ToList()
                            };
                        }).ToList();
                        newInfoboards.ForEach(ii =>
                        {
                            tenantContext.Infoboards.Add(ii);
                        });
                        await tenantContext.CommitAsync();

                    }
                }
                var message = Mailer.MergeTemplate("en", "Welcome", new Dictionary<string, string> { { "Username",user.UserName }, { "Password",password }});
                await Mailer.SendMail(new List<Infoveave.Mailer.Recipient>() { new Mailer.Recipient { DisplayName = user.UserName, Email = user.Email } }, "Welcome to Infoveave", message);
            }
            catch (Exception)
            {
            }
            return new User { Id = newUser.Id, RoleId = newUser.UserRoleId, UserName = newUser.UserName, FirstName = newUser.FirstName, LastName = user.LastName, Email = user.Email, CreatedOn = user.CreatedOn };
        }

        /// <summary>
        /// Update User
        /// </summary>
        /// <param name="id"></param>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(long id, [FromBody]User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var existingUser = await tenantContext.Users.GetAll().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
            if (existingUser == null)
            {
                throw new Exception("AUTH-U003");
            }
            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.Email = user.Email.ToLower();
            existingUser.ImagePath = user.ImagePath;
            existingUser.UserRoleId = (user.RoleId != 0) ? user.RoleId : existingUser.UserRoleId;
            existingUser.Context = user.UserContext;
            existingUser.Language = user.Language;
            tenantContext.Users.Update(existingUser);
            await tenantContext.CommitAsync();
            return Ok();
        }

        /// <summary>
        /// Update Profile
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpPut("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile([FromBody]User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var existingUser = await tenantContext.Users.GetAll().FirstOrDefaultAsync(i => i.Id == CurrentUser.Id, cancellationToken);
            if (existingUser == null)
            {
                throw new Exception("AUTH-U003");
            }
            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.Email = user.Email.ToLower();
            existingUser.UserRoleId = (user.RoleId != 0) ? user.RoleId : existingUser.UserRoleId;
            existingUser.Language = user.Language;
            tenantContext.Users.Update(existingUser);
            await tenantContext.CommitAsync();
            return Ok();
        }


        /// <summary>
        /// Reset User Password (Mail)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpGet("{id}/ResetPassword")]
        public async Task<IActionResult> ResetPassword(long id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var user = await tenantContext.Users.GetAll().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
            if (user == null)
            {
                throw new Exception("AUTH-U003");
            }
            var password = RandomString();
            user.SetPassword(password);
            tenantContext.Users.Update(user);
            await tenantContext.CommitAsync();
            var message = Mailer.MergeTemplate("en", "Welcome", new Dictionary<string, string>
            {
                { "Domain",CurrentUser.Tenant },
                { "Username",user.UserName },
                { "Password",password },
            });
            await Mailer.SendMail(new List<Infoveave.Mailer.Recipient>()
            {
               new Mailer.Recipient { DisplayName = user.UserName, Email = user.Email },
            }, "Reset Password", message);
            return Ok();
        }


        /// <summary>
        /// Forgot Password
        /// </summary>
        /// <param name="userP"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [AllowAnonymous]
        [HttpPost("ForgotPassword")]
        public async Task<User> ForgotPassword([FromBody]User userP, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(userP.Tenant);
            var user = await tenantContext.Users.GetAll().Where(u => u.UserName.ToLower() == userP.UserName.ToLower()).FirstOrDefaultAsync();
            if (user == null)
            {
                throw new Exception("AUTH-U003");
            }
            var password = RandomString();
            user.SetPassword(password);
            user.IsLockedOut = false;
            tenantContext.Users.Update(user);
            await tenantContext.CommitAsync();
            var message = Mailer.MergeTemplate("en", "Welcome", new Dictionary<string, string>
            {
                { "Domain",userP.Tenant },
                { "Username",userP.UserName },
                { "Password",password },
            });
            await Mailer.SendMail(new List<Infoveave.Mailer.Recipient>()
            {
               new Mailer.Recipient { DisplayName = user.UserName, Email = user.Email },
            }, "Reset Password", message);
            var email = user.Email.Split('@')[0].First() + "********" + user.Email.Split('@')[0].Last() + "@" + user.Email.Split('@')[1];
            return new User
            {
                UserName = userP.UserName,
                Email = email
            };
        }

        /// <summary>
        /// Unlock Locked User
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpGet("{id}/UnlockUser")]
        public async Task<IActionResult> UnlockUser(long id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var user = await tenantContext.Users.GetAll().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
            if (user == null)
            {
                throw new Exception("AUTH-U003");
            }
            user.UnlockUser();
            tenantContext.Users.Update(user);
            await tenantContext.CommitAsync();
            return Ok();
        }


        /// <summary>
        /// Delete User
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(long id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var user = await tenantContext.Users.GetAll().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
            if (user == null)
            {
                throw new Exception("AUTH-U003");
            }
            tenantContext.Users.Delete(user);
            await tenantContext.CommitAsync();
            return Ok();
        }
        private string RandomString()
        {
            const string chars = "ABCDEFGHJKMNPQRSTUVWXYZ123456789abcdefghjkmnpqrstuvwxyz";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Get All Available Permissions
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("Permissions")]
        [Versions("v2")]
        public async Task<string> GetUserPermissions(CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var user = await tenantContext.Users.GetAll().FirstOrDefaultAsync(i => i.Id == CurrentUser.Id, cancellationToken);
            var role = await tenantContext.UserRoles.GetAll().FirstOrDefaultAsync(i => i.Id == user.UserRoleId, cancellationToken);
            if (role.Permissions == "*")
            {
                var permissions = new List<Permission>();
                permissions = JsonConvert.DeserializeObject<List<Permission>>(System.IO.File.ReadAllText(System.IO.Path.Combine(_appEnvironment.ContentRootPath, "permission.json")));
                var allPermissions = new List<string>();
                foreach (var permission in permissions)
                {
                    foreach (var view in permission.views)
                    {
                        foreach (var action in view.actions)
                        {
                            allPermissions.Add(permission.module + "|" + view.view + "|" + action);
                        }
                    }
                }
                return JsonConvert.SerializeObject(allPermissions);
            }
            return role.Permissions;
        }

        /// <summary>
        /// All User Emails
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>

        [HttpGet("Emails")]
        [Versions("v2")]
        public async Task<List<string>> GetUserEmails(CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var users = await tenantContext.Users.GetAll().Select(x => x.Email).ToListAsync(cancellationToken);
            var data = new List<string>();
            return users;
        }

        /// <summary>
        /// Upload Profile Photo
        /// </summary>
        /// <param name="id"></param>
        /// <param name="file"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpPost("{id}/ProfileImage")]
        public async Task<UploadImage> UploadProfilePhoto(long id, IFormFile file, CancellationToken cancellationToken = default(CancellationToken))
        {
            string serverUploadFolder = Configuration.Application.Paths["Images"];
            var filename = ContentDispositionHeaderValue
                       .Parse(file.ContentDisposition)
                       .FileName
                       .Trim('"');
            var extension = filename.Split('.')[1].ToLower();
            var fileName = "";
            if (extension.Contains("png") || extension.Contains("jpg"))
            {
                fileName = string.Format("{0}.{1}.{2}", "Images", Guid.NewGuid(), filename.Split('.')[1]);
                await file.SaveAsAsync(Path.Combine(serverUploadFolder, CurrentUser.Tenant, fileName));
                var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
                var existingUser = await tenantContext.Users.GetAll().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
                if (existingUser == null)
                {
                    throw new Exception("AUTH-U003");
                }
                existingUser.ImagePath = fileName;
                tenantContext.Users.Update(existingUser);
                await tenantContext.CommitAsync();
            }
            else
            {
                throw new Exception("AUTH-U005");
            }

            return new UploadImage()
            {
                name = fileName,
            };
        }

        /// <summary>
        /// Get Logged In User Information
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("CurrentUser")]
        [Versions("v2")]
        public async Task<User> GetCurrentUser(CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var user = await tenantContext.Users.GetAll().FirstOrDefaultAsync(i => i.Id == CurrentUser.Id, cancellationToken);
            return new User()
            {
                Id = user.Id,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                CreatedOn = user.CreatedOn,
                RoleId = user.UserRoleId,
                ImagePath = (string.IsNullOrEmpty(user.ImagePath)) ? "" : user.ImagePath,
                Language = user.Language,
                Version = Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationVersion

            };
        }

        /// <summary>
        /// Change Password
        /// </summary>
        /// <param name="model"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpPut("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody]ChangePassword model, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var user = await tenantContext.Users.GetAll().FirstOrDefaultAsync(i => i.Id == CurrentUser.Id, cancellationToken);
            if (user == null)
            {
                throw new Exception("AUTH-R003");
            }
            if (user.ValidatePassword(model.OldPassword))
            {
                user.SetPassword(model.NewPassword);
                tenantContext.Users.Update(user);
                await tenantContext.CommitAsync();
            }
            else
            {
                throw new Exception("AUTH-R006");
            }
            return Ok();
        }

        /// <summary>
        /// Get Redirection Route
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("RouteResolver")]
        [Versions("v2")]
        public async Task<dynamic> GetRoute(CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var user = await tenantContext.Users.GetAll().FirstOrDefaultAsync(i => i.Id == CurrentUser.Id, cancellationToken);
            var role = await tenantContext.UserRoles.GetAll().FirstOrDefaultAsync(i => i.Id == user.UserRoleId, cancellationToken);
            var infoboards = await tenantContext.Infoboards.GetAll().Where(i => i.CreatedBy == user.Id).OrderBy(i => i.SortOrder).ToListAsync(cancellationToken);
            var routeResolver = "app/profile";
            if (infoboards != null && infoboards.Count > 0 && (role.Permissions == "*" || role.Permissions.Contains("Infoboards|List")))
                routeResolver = "app/infoboards/" + infoboards.FirstOrDefault().Id;
            else if (role.Permissions == "*" || role.Permissions.Contains("GoogleDataSource|List") || role.Permissions.Contains("InfoveaveDataSource|List"))
                routeResolver = "app/dataSourceManager";
            return new { routeResolver };
        }


        /// <summary>
        /// Get Available DataSources
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpGet("DataSources")]
        public async Task<IEnumerable<DataSource>> GetDataSources(CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var dataSources = await tenantContext.DataSources.GetAll().Where(d => d.IsPublic == true).ToListAsync(cancellationToken);
            var dataSourceIds = dataSources.Select(d => d.Id).ToList();
            var measures = await tenantContext.Measures.GetAll().Where(m => dataSourceIds.Contains(m.DataSourceId)).ToListAsync(cancellationToken);
            var dimensions = await tenantContext.Dimensions.GetAll().Where(d => dataSourceIds.Contains(d.DataSourceId)).ToListAsync(cancellationToken);
            return dataSources.Select(ds => new DataSource
            {
                Id = ds.Id,
                Name = ds.Name,
                Type = ds.ServerType,
                Measures = measures.Where(m => m.DataSourceId == ds.Id).Select(m => new Measure
                {
                    Id = m.Id,
                    Name = m.Name,
                    Query = m.Query,
                    DataSourceId = m.DataSourceId
                }),
                Dimensions = dimensions.Where(d => d.DataSourceId == ds.Id).Select(d => new Dimension
                {
                    Id = d.Id,
                    Name = d.Name,
                    Query = d.Query,
                    IsDate = d.IsDate,
                    DataSourceId = d.DataSourceId
                })
            });
        }

        /// <summary>
        /// Dimension Items of a DataSource
        /// </summary>
        /// <param name="dataSourceId"></param>
        /// <param name="id"></param>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpGet("DataSources/{dataSourceId}/Dimensions/{id}")]
        public async Task<List<string>> GetDimensionItems(long dataSourceId, long id, string query, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var dataSource = await tenantContext.DataSources.GetAll().FirstOrDefaultAsync(i => i.Id == dataSourceId, cancellationToken);
            var dimension = await tenantContext.Dimensions.GetAll().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
            var adapterToUse = (dataSource.ServerType == "mondrianDirect") ? "mondrianService" : dataSource.ServerType;
            var olapAdapter = Helpers.Adapters.OlapAdapters[adapterToUse];
            var connection = new AdapterFramework.OlapConnection
            {
                Server = dataSource.Server,
                Database = dataSource.AnalysisDataBase,
                Cube = dataSource.Cube,
                AdditionalData = (dataSource.ServerType == "mondrianDirect") ? dataSource.ConnectionString : null,
            };
            connection = await OlapAdapterHelpers.TransformConnectionForAdapter(dataSource.ServerType, CurrentUser.Tenant, dataSource.Id, connection, Configuration, HttpContext, Request, cancellationToken);
            List<string> data = await CacheProvider.GetDimesnionItemsAsync(olapAdapter, connection, dimension.Query, query, cancellationToken);
            var filtered = data.ToList();
            if (!string.IsNullOrEmpty(query))
            {
                filtered = filtered.Where(d => d.ToLower().Contains(query.ToLower())).Take(100).ToList();
            }
            var result = filtered.Select(f => f).ToList();
            result.Insert(0, "All");
            return result;
        }

        /// <summary>
        /// User Profile Image
        /// </summary>
        /// <param name="tenant"></param>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [AllowAnonymous]
        [HttpGet("{tenant}/{id}/ProfileImage")]
        public async Task<FileResult> GetProfileImage(string tenant, long id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(tenant);
            var user = await tenantContext.Users.GetAll().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
            Response.Headers.Add("Content-Disposition", new Microsoft.Extensions.Primitives.StringValues("Inline:true"));
            if (!string.IsNullOrEmpty(user.ImagePath) && System.IO.File.Exists(System.IO.Path.Combine(Configuration.Application.Paths["Images"], tenant, user.ImagePath)))
            {
                var image = System.IO.File.ReadAllBytes(System.IO.Path.Combine(Configuration.Application.Paths["Images"], tenant, user.ImagePath));
                return File(image, "application/octet-stream");
            }
            else
            {
                var image = System.IO.File.ReadAllBytes(System.IO.Path.Combine(Configuration.Application.Paths["Images"], "defaultAvatar.png"));
                return File(image, "image/png");
            }
        }

        /// <summary>
        /// Create Administrator for a Single Tenanted Setup
        /// </summary>
        /// <param name="tenant"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [AllowAnonymous]
        [HttpPost("CreateAdministrator")]
        public async Task<IActionResult> CreateAdministrator([FromBody]TenantSignup tenant, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantRepo = TenantContext.CreateTenantRepository("default");
            var organisation = new Models.Organisation()
            {
                Language = "en",
                Name = tenant.OrganisationName,
                MaximumUsers = 5,
                StarterPack = 0,
                CreatedOn = DateTime.Now
            };
            var role = new Models.UserRole()
            {
                Name = "Administrator",
                CreatedOn = DateTime.Now,
                CreatedBy = 0,
                Permissions = "*",
            };
            tenantRepo.Organisations.Add(organisation);
            tenantRepo.UserRoles.Add(role);
            await tenantRepo.CommitAsync();
            var user = new Models.User()
            {
                UserName = tenant.UserName.ToLower(),
                Email = tenant.UserEmail.ToLower(),
                CreatedOn = DateTime.Now,
                FirstName = tenant.OrganisationName,
                LastName = "Administrator",
                IsArchived = false,
                IsLockedOut = false,
                LoginAttempts = 0,
                Language = "en",
                UserRoleId = role.Id
            };
            user.SetPassword(tenant.Password);
            tenantRepo.Users.Add(user);
            await tenantRepo.CommitAsync();

            return new ObjectResult(new
            {
                Message = "Created Successfully",
                Name = tenant.OrganisationName,
                Domain = tenant.DomainName,
                UserName = tenant.UserName,
                UserEmail = tenant.UserEmail
            });
        }
    }

}
