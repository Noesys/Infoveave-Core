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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Infoveave.Helpers
{
    public class HandleApiExceptionAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {

            var result = new ObjectResult(new
            {
                context.Exception.Message,
                AdditionalInformation = context.Exception,
                context.Exception.StackTrace
            })
            { StatusCode = 400 };
            context.Result = result;
        }
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member