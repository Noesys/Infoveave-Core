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
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Win32.SafeHandles;
using Swashbuckle.Swagger.Model;
using Swashbuckle.SwaggerGen.Generator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Infoveave.Helpers
{
    public class CancellationTokenOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var apiDescription = context.ApiDescription;
            var excludedParameters = apiDescription.ParameterDescriptions
                .Where(p => p.Name == "versions" || p.Name == "version" || p.ModelMetadata.ContainerType == typeof(CancellationToken) || p.ModelMetadata.ContainerType == typeof(WaitHandle) || p.ModelMetadata.ContainerType == typeof(SafeWaitHandle))
                .Select(p => operation.Parameters.FirstOrDefault(operationParam => operationParam.Name == p.Name))
                .ToList();

            foreach (var parameter in excludedParameters)
                operation.Parameters.Remove(parameter);
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member