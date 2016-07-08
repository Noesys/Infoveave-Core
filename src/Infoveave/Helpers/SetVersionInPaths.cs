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
using Swashbuckle.Swagger.Model;
using Swashbuckle.SwaggerGen;
using Swashbuckle.SwaggerGen.Generator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Infoveave.Helpers
{
    public class SetVersionInPaths : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Paths = swaggerDoc.Paths.ToDictionary(
                entry => entry.Key.Replace("{version}", swaggerDoc.Info.Version),
                entry =>
                {
                    var pathItem = entry.Value;
                    RemoveVersionParamFrom(pathItem.Get);
                    RemoveVersionParamFrom(pathItem.Put);
                    RemoveVersionParamFrom(pathItem.Post);
                    RemoveVersionParamFrom(pathItem.Delete);
                    RemoveVersionParamFrom(pathItem.Options);
                    RemoveVersionParamFrom(pathItem.Head);
                    RemoveVersionParamFrom(pathItem.Patch);
                    return pathItem;
                });
        }

        private void RemoveVersionParamFrom(Operation operation)
        {
            if (operation == null || operation.Parameters == null) return;

            var versionParam = operation.Parameters.FirstOrDefault(param => param.Name == "version");
            if (versionParam == null) return;

            operation.Parameters.Remove(versionParam);
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member