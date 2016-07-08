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
using Infoveave.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using System.Linq;
using System.Threading.Tasks;

namespace Infoveave.Helpers
{
    /// <summary>
    /// Logger
    /// </summary>
    public class LoggerMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Log
        /// </summary>
        /// <param name="next"></param>
        /// <param name="loggerFactory"></param>
        public LoggerMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
        }

        /// <summary>
        /// Execute
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            var user = context.User;
            if (user == null || !user.Claims.Any())
            {
                LogContext.PushProperty("Tenant", "Anonymous");
                LogContext.PushProperty("Username", "Anonymous");
            }
            else
            {
                LogContext.PushProperty("Tenant", user.Claims.First(c => c.Type == Constants.Tenant).Value);
                LogContext.PushProperty("Username", user.Claims.First(c => c.Type == Constants.UserName).Value);
            }
            await _next.Invoke(context);
            
        }
    }
}