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
using Infoveave.AdapterFramework;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
#pragma warning disable CS1591
namespace Infoveave.Helpers
{
    public class OlapAdapterHelpers
    {
        public async static Task<AdapterFramework.OlapConnection> TransformConnectionForAdapter(string adapter, string tenant, long dataSourceId,AdapterFramework.OlapConnection connection, ApplicationConfiguration configuration, HttpContext httpContext, HttpRequest request, CancellationToken cancellationToken)
        {
            await Task.Delay(0);
            if (adapter == "mondrianService")
            {
                var sqlConnection = JsonConvert.DeserializeObject<SQLConnection>(connection.Server);
                if (sqlConnection.Type == "sqlite")
                {
                    sqlConnection.Server = Path.Combine(configuration.Application.AnalyticsData.ConnectionString, tenant);
                }
                connection.Server = JsonConvert.SerializeObject(sqlConnection);
                var authority = "http://" + request.Host;
                connection.Database = new Uri(authority + "/Services/" + tenant + "/" + dataSourceId + "/Mondrian4").ToString();
                connection.AdditionalData = string.Format("http://{0}:{1}", configuration.Application.MondrianService.Endpoint, configuration.Application.MondrianService.Port);
                return connection;
            }
#if NET461
            if (adapter == "googleAnalytics")
            {
                connection.AdditionalData = await GoogleAuth.GetAuthorisation(configuration.Application.Paths["Configurations"], tenant, connection.Server, cancellationToken);
            }
#endif
            if (adapter == "mondrianDirect")
            { 
                var sqlConnection = JsonConvert.DeserializeObject<SQLConnection>(connection.Server);
                connection.Server = JsonConvert.SerializeObject(sqlConnection);
                connection.Database = $"file://{Path.Combine(configuration.Application.Paths["Reports"], tenant, connection.AdditionalData)}";
                connection.AdditionalData = string.Format("http://{0}:{1}", configuration.Application.MondrianService.Endpoint, configuration.Application.MondrianService.Port);
                return connection;
            }
            return connection;
        }

        public async static Task<SQLConnection> TransformSQLConnection(SQLConnection connection, string tenant, ApplicationConfiguration configuration)
        {
            await Task.Delay(0);
            if (connection.Type == "sqlite")
            {
                connection.Server = Path.Combine(configuration.Application.AnalyticsData.ConnectionString, tenant);
            }
            if (connection.Type == "druid")
            {
                connection.Server = tenant;
            }
            return connection;
        }

    }
}
#pragma warning restore CS1591
