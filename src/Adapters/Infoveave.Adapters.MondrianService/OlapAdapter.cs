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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace Infoveave.Adapters.MondrianService
{
    public class OlapAdapter : IOlapAdapter
    {
        private readonly ILogger logger;
        public OlapAdapter(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger("Adapters:OLAP:MondrianService");
        }

        public string Provider { get { return "mondrianService"; } } 

        public bool SupportsCache { get { return true; } }
        
        public bool SupportsDatabaseList { get { return false; } }

        public async Task<ExecuteQueryResult> ExecuteQueryAsync(OlapConnection connection, AnalysisQuery query)
        {
            var mdxQuery = query.GetQuery();
            var data =  await this.ExecuteQueryAsyncInternal(connection, mdxQuery);
            if (data.Count == 0) return new ExecuteQueryResult { FromCache = false, Query = query, Result = data };
            for (int i = 0; i < query.Measures.Count; i++)
            {
                for (int j = 1; j < data.Count; j++)
                {
                    double res = 0;
                    Double.TryParse(data[j][data[0].Count - i - 1], out res);
                    data[j][data[0].Count - i - 1] = res;
                }
                
            }
            for (int i = 0; i < query.Dimensions.Count; i++)
            {
                for (int j = 1; j < data.Count; j++)
                {
                    if (string.IsNullOrEmpty(data[j][i].ToString()))
                    {
                        data[j][i] = "Unknown Member";
                    }
                }
            }
            return new ExecuteQueryResult { FromCache = false, Query = query, Result = data };
        }

        public async Task<List<List<dynamic>>> ExecuteQueryAsyncInternal(OlapConnection connection, string mdxQuery)
        {
            HttpClient client = new HttpClient();
            var sqlConnection = JsonConvert.DeserializeObject<SQLConnection>(connection.Server);
            logger.LogInformation("MDXQuery: {MdxQuery}", mdxQuery);
            var queryObject = new MondrianQueryObject
            {
                Server = sqlConnection.Server,
                DatabaseType = sqlConnection.Type,
                Username = sqlConnection.Username,
                Password = sqlConnection.Password,
                Port = sqlConnection.Port,
                MdxQuery = mdxQuery,
                Database = sqlConnection.Database,
                Schema = connection.Database,
            };
            logger.LogDebug("Mondrian Request", queryObject);
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            HttpResponseMessage response = await client.PostAsync(connection.AdditionalData + "/executeQuery", new StringContent(JsonConvert.SerializeObject(queryObject, settings), Encoding.UTF8, "application/json"));
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var data = JsonConvert.DeserializeObject<List<List<dynamic>>>(await response.Content.ReadAsStringAsync());
                logger.LogDebug("Mondrian Response", data);
                return data;           
            }
            var error = await response.Content.ReadAsStringAsync();
            logger.LogError("Mondrian Response", error);
            throw new Exception("Mondiran-Error", new Exception(error));
        }

        public async Task<bool> FlushCache(OlapConnection connection)
        {
            HttpClient client = new HttpClient();
            var sqlConnection = JsonConvert.DeserializeObject<SQLConnection>(connection.Server);
            var queryObject = new MondrianQueryObject
            {
                Server = sqlConnection.Server,
                DatabaseType = sqlConnection.Type,
                Username = sqlConnection.Username,
                Password = sqlConnection.Password,
                Cube = connection.Cube,
                Port = sqlConnection.Port,
                MdxQuery = null,
                Database = sqlConnection.Database,
                Schema = connection.Database,
            };
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            HttpResponseMessage response = await client.PostAsync(connection.AdditionalData + "/cleanCache", new StringContent(JsonConvert.SerializeObject(queryObject, settings), Encoding.UTF8, "application/json"));
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return true;
            }
            throw new Exception("Mondiran-Error", new Exception(await response.Content.ReadAsStringAsync()));
        }

        public Task<List<string>> GetCubesAsync(OlapConnection connection)
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> GetDatabasesAsync(OlapConnection connection)
        {
            throw new NotImplementedException();
        }

        public async Task<List<string>> GetDimensionItemsAsync(OlapConnection connection, string hierarcy, string filter = null)
        {
            var mdx = string.Format(" With Member [Measures].[DimensionKey] as {0}.CurrentMember.Properties(\"KEY\") Select {{ [Measures].[DimensionKey] }} on Columns, {{ {0}.Children }} on Rows from [{1}]", hierarcy, connection.Cube);
            var resultSet = await this.ExecuteQueryAsyncInternal(connection, mdx);
            var data = new List<string>();
            for (int i = 1; i < resultSet.Count; i++)
            {
                data.Add(resultSet[i][0]);
            }
            return data;
        }

        public async Task<List<OlapDimensionGroup>> GetDimensionsAsync(OlapConnection connection)
        {
            HttpClient client = new HttpClient();
            var sqlConnection = JsonConvert.DeserializeObject<SQLConnection>(connection.Server);
            var queryObject = new MondrianQueryObject
            {
                Server = sqlConnection.Server,
                DatabaseType = sqlConnection.Type,
                Username = sqlConnection.Username,
                Password = sqlConnection.Password,
                Port = sqlConnection.Port,
                Cube = connection.Cube,
                MdxQuery = null,
                Database = sqlConnection.Database,
                Schema = connection.Database,
            };
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            HttpResponseMessage response = await client.PostAsync(connection.AdditionalData + "/dimensions", new StringContent(JsonConvert.SerializeObject(queryObject, settings), Encoding.UTF8, "application/json"));
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var data = JsonConvert.DeserializeObject<List<OlapDimensionGroup>>(await response.Content.ReadAsStringAsync());
                return data;
            }
            throw new Exception("Mondiran-Error", new Exception(await response.Content.ReadAsStringAsync()));
        }

        public async Task<Dictionary<string, string>> GetMeasuresAsync(OlapConnection connection)
        {
            HttpClient client = new HttpClient();
            var sqlConnection = JsonConvert.DeserializeObject<SQLConnection>(connection.Server);
            var queryObject = new MondrianQueryObject
            {
                Server = sqlConnection.Server,
                DatabaseType = sqlConnection.Type,
                Username = sqlConnection.Username,
                Password = sqlConnection.Password,
                Port = sqlConnection.Port,
                Cube = connection.Cube,
                MdxQuery = null,
                Database = sqlConnection.Database,
                Schema = connection.Database,
            };
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            HttpResponseMessage response = await client.PostAsync(connection.AdditionalData + "/measures", new StringContent(JsonConvert.SerializeObject(queryObject, settings), Encoding.UTF8, "application/json"));
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var data = JsonConvert.DeserializeObject<List<MondrianMeasure>>(await response.Content.ReadAsStringAsync());
                return data.ToDictionary(d => d.Caption, d => d.UniqueName);
            }
            throw new Exception("Mondiran-Error", new Exception(await response.Content.ReadAsStringAsync()));
        }

        public Task<KeyValuePair<bool, string>> TestConnectivityAsync(OlapConnection connection)
        {
            throw new NotImplementedException();
        }
    }

    public class MondrianMeasure
    {
        public string Caption { get; set; }
        public string UniqueName { get; set; }
    }

    public class MondrianQueryObject
    {
        public string Schema {get;set;}
        public string DatabaseType { get; set; }
        public string Server { get; set; }
        public long Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }
        public string MdxQuery { get; set; }
        public string Cube { get; set; }
    }
}
