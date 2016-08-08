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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Infoveave.Controllers
{

    /// <summary>
    /// Authorisation Controller
    /// </summary>
    [Route("/Connect")]
    public class ConnectController : BaseController
    {
        private Microsoft.IdentityModel.Tokens.SigningCredentials SigningParamters { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="signingParamters"></param>
        public ConnectController(ITenantContext context, Microsoft.IdentityModel.Tokens.SigningCredentials signingParamters) : base(context)
        {
            SigningParamters = signingParamters;
        }

        /// <summary>
        /// Login
        /// </summary>
        /// <remarks>
        /// Issues a Bearer Token which can be used for all subsequnt calls.
        /// </remarks>
        /// <param name="context">Refer Schema</param>
        /// <returns>Bearer Token for the given User</returns>
        [HttpPost("Token")]
        public async Task<AuthenticationResult> Login(LoginRequest context)
        {

            var tenant = context.Acr_values.Split(':')[1];
            var repository = TenantContext.GetTenantRepository(tenant);
            var userNameLower = context.Username.ToLower();
            var user = await repository.Users.GetAll().FirstOrDefaultAsync(u => u.UserName == userNameLower || u.Email == userNameLower);
            if (user == null)
            {
                throw new Exception("AUTH-0001", new Exception("User not registered in Tenant"));
            }
            if (user.IsLockedOut)
            {
                throw new Exception("AUTH-0002", new Exception("User is Locked out because of too mant invalid password attempts"));
            }
            if (!user.ValidatePassword(context.Password))
            {
                user.LoginAttempts += 1;
                if (user.LoginAttempts == 5)
                {
                    user.IsLockedOut = true;
                    user.LockoutDate = DateTime.Now;
                }
                await repository.CommitAsync();
                throw new Exception("AUTH-0003", new Exception("Invalid Password"));
            }
            else
            {
                user.LoginAttempts = 0;
                user.LastLoginDateTime = DateTime.Now;
                await repository.CommitAsync();
                var handler = new JwtSecurityTokenHandler();
                var claims = new List<Claim>()
                {
                    new Claim(Constants.Id,user.Id.ToString()),
                    new Claim(Constants.Tenant,tenant),
                    new Claim(Constants.UserName,user.UserName),
                    new Claim(Constants.Email, user.Email),
                    new Claim(Constants.Role,user.UserRoleId.ToString())
                };
                ClaimsIdentity identity = new ClaimsIdentity(new GenericIdentity(user.Id.ToString(), "Bearer"), claims);
                var securityToken = handler.CreateToken(new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor()
                {
                    
                
                        Issuer= "Infoveave",
                        Audience= "Infoveave",
                        SigningCredentials= SigningParamters,
                        Subject = identity,
                        Expires =  DateTime.Now.AddDays(1),
                        IssuedAt = DateTime.Now
                    });
                var token = handler.WriteToken(securityToken);
                return new AuthenticationResult
                {
                    Access_token = token,
                    Expires_in = 24 * 60 * 60,
                    Token_type = "Bearer"
                };
            }
            
        }

        /// <summary>
        /// User Info
        /// </summary>
        /// <remarks>
        /// Claims of the Bearer Token
        /// </remarks>
        /// <returns>Claims of the User (Refer Schema)</returns>
        [Authorize("Bearer")]
        [HttpGet("UserInfo")]
        public Task<ViewModels.UserInfo> UserInfo()
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var user = tenantContext.Users.GetAll().Where(u => u.Id == CurrentUser.Id).FirstOrDefault();
            var userInfo = CurrentUser;
            return Task.FromResult(userInfo);
        }
    }
}
