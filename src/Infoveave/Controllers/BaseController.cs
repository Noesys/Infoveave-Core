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
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Infoveave.Controllers
{
    public class BaseController : Controller
    {
        protected ITenantContext TenantContext { get; private set; }
        protected ApplicationConfiguration Configuration { get; private set; }
        protected CacheProvider.ICacheProvider CacheProvider { get; private set; }

        public BaseController(ITenantContext tenantContext,
            IOptions<ApplicationConfiguration> configuration = null,
            CacheProvider.ICacheProvider cacheProvider = null
            )
        {
            Configuration = (configuration != null) ? configuration.Value : null;
            TenantContext = tenantContext;
            CacheProvider = cacheProvider;
        }

        protected ViewModels.UserInfo CurrentUser
        {
            get
            {
                if (User == null || !User.Claims.Any()) return null;
                return new ViewModels.UserInfo
                {
                    Id = long.Parse(User.Claims.First(c => c.Type == Constants.Id).Value),
                    UserName = User.Claims.First(c => c.Type == Constants.UserName).Value,
                    Tenant = User.Claims.First(c => c.Type == Constants.Tenant).Value,
                    Email = User.Claims.First(c => c.Type == Constants.Email).Value,
                    RoleId = long.Parse(User.Claims.First(c => c.Type == Constants.Role).Value),
                };
            }
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member