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
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Infoveave.CacheProvider
{
    public class CacheProvider : ICacheProvider
    {
        public ConnectionMultiplexer redis;
        protected bool Enabled { get; private set; }
        protected string Host { get; private set; }
        protected int Port { get; private set; }

        public CacheProvider() { }


        public void ConfigureCaching(bool enabled, string host, int port)
        {
            if (!enabled) return;
            Enabled = enabled;
            Host = host;
            Port = port;
            redis = ConnectionMultiplexer.Connect(new ConfigurationOptions
            {
                EndPoints =
                {
                    {host,port }
                },
                AllowAdmin = true
            });
        }


        #region "OLAP Adapter Related"
        public async Task RefreshDataSourceAsync(IOlapAdapter adapter, OlapConnection connection)
        {
            if (Enabled && adapter.SupportsCache)
            {
                IDatabase db = redis.GetDatabase();
                IServer rServer = redis.GetServer(Host, Port);
                await rServer.FlushAllDatabasesAsync();
                var key = string.Format("{0}_{1}_{2}", connection.Server, connection.Database, connection.Cube);
                var allSaved = db.HashKeys(key);
                foreach (var item in allSaved)
                {
                    var query = new AnalysisQuery(connection.Cube);
                    query.ApplyFromSerialized(JsonConvert.DeserializeObject<AnalysisQuerySerializable>(item));
                    var data = await adapter.ExecuteQueryAsync(connection,query);
                    await db.HashSetAsync(key, item, JsonConvert.SerializeObject(data));
                }
            }
        }

        public async Task<ExecuteQueryResult> ExecuteQueryAsync(IOlapAdapter adapter, OlapConnection connection, AnalysisQuery mdxquery, CancellationToken cancellationToken)
        {
            await Task.Delay(0);
            if (Enabled && adapter.SupportsCache)
            {
                IDatabase db = redis.GetDatabase();
                var key = string.Format("{0}_{1}_{2}", connection.Server, connection.Database, connection.Cube);
                var existing = db.HashGet(key, JsonConvert.SerializeObject(mdxquery.GetSerializable()));
                if (existing.IsNull)
                {
                    var data = await adapter.ExecuteQueryAsync(connection, mdxquery);
                    await db.HashSetAsync(key, JsonConvert.SerializeObject(mdxquery.GetSerializable()), JsonConvert.SerializeObject(data.Result));
                    return new ExecuteQueryResult { FromCache = false, Query = mdxquery, Result = data.Result };
                }
                else
                {
                    return new ExecuteQueryResult { FromCache = true, Query = mdxquery, Result = JsonConvert.DeserializeObject<List<List<dynamic>>>(existing) };
                }
            }
            else
            {
                return await adapter.ExecuteQueryAsync(connection, mdxquery);
            }
        }
        public async Task<List<string>> GetDimesnionItemsAsync(IOlapAdapter adapter, OlapConnection connection, string query, string filter = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (Enabled && adapter.SupportsCache)
            {
                IDatabase db = redis.GetDatabase();
                var key = string.Format("{0}_{1}_{2}", connection.Server, connection.Database, connection.Cube);
                var existing = await db.HashGetAsync(key, JsonConvert.SerializeObject(query));
                if (existing.IsNull)
                {
                    var data = await adapter.GetDimensionItemsAsync(connection, query);
                    await db.HashSetAsync(key, JsonConvert.SerializeObject(query), JsonConvert.SerializeObject(data));
                    return data;
                }
                else
                {
                    return JsonConvert.DeserializeObject<List<string>>(existing);
                }
            }
            else
            {
                return await adapter.GetDimensionItemsAsync(connection, query, filter);
            }
        }

        #endregion


        #region "SQL Adapter Related"

        public async Task<KeyValuePair<bool, List<Dictionary<string, dynamic>>>> ExecuteQueryAsync(ISQLAdapter adapter, SQLConnection connectionString, string query)
        {
            if (Enabled)
            {
                IDatabase db = redis.GetDatabase();
                var key = string.Format("{0}_{1}", connectionString.Server, connectionString.Database);
                var existing = await db.HashGetAsync(key, CreateMD5(query));

                if (existing.IsNull)
                {
                    var data = await adapter.ExecuteQueryAsync(connectionString, query);
                    await db.HashSetAsync(key, query, JsonConvert.SerializeObject(data));
                    return new KeyValuePair<bool, List<Dictionary<string, dynamic>>>(false, data);
                }
                else
                {
                    return new KeyValuePair<bool, List<Dictionary<string, dynamic>>>(true, JsonConvert.DeserializeObject<List<Dictionary<string, dynamic>>>(existing));
                }
            }
            else
            {
                var data = await adapter.ExecuteQueryAsync(connectionString, query);
                return new KeyValuePair<bool, List<Dictionary<string, dynamic>>>(false, data);
            }
        }
        public async Task<KeyValuePair<bool,IList<string>>> GetTablesAsync(ISQLAdapter adapter, SQLConnection connectionString)
        {
            string error = string.Empty;
            if (Enabled)
            {
                IDatabase db = redis.GetDatabase();
                var key = string.Format("{0}_{1}", connectionString.Server, connectionString.Database);
                var existing = await db.HashGetAsync(key, "Tables");

                if (existing.IsNull)
                {
                    var tables = await adapter.GetTablesAsync(connectionString);
                    var set = await db.HashSetAsync(key, "Tables", JsonConvert.SerializeObject(tables));
                    return new KeyValuePair<bool, IList<string>>(false,tables);
                }
                else
                {
                    return new KeyValuePair<bool, IList<string>>(true,JsonConvert.DeserializeObject<IList<string>>(existing));
                }
            }
            else
            {
                var tables = await adapter.GetTablesAsync(connectionString);
                return new KeyValuePair<bool, IList<string>>(false, tables);
            }
        }

        public async Task<KeyValuePair<bool,Dictionary<string, string>>> GetColumnsAsync(ISQLAdapter adapter, SQLConnection connectionString, string table)
        {
            string error = string.Empty;
            if (Enabled)
            {
                IDatabase db = redis.GetDatabase();
                var key = string.Format("{0}_{1}", connectionString.Server, connectionString.Database);
                var existing = await db.HashGetAsync(key, "Columns_" + table);

                if (existing.IsNull)
                {
                    var columnInfo = await adapter.GetColumnsAsync(connectionString, table);
                    var set = await db.HashSetAsync(key, "Columns_" + table, JsonConvert.SerializeObject(columnInfo));
                    return new KeyValuePair<bool, Dictionary<string, string>>(false,columnInfo);
                }
                else
                {
                    return new KeyValuePair<bool, Dictionary<string, string>>(true, JsonConvert.DeserializeObject<Dictionary<string, string>>(existing));
                }
            }
            else
            {
                var columnInfo = await adapter.GetColumnsAsync(connectionString, table);
                return new KeyValuePair<bool, Dictionary<string, string>>(false, columnInfo);
            }
        }
   
        #endregion






        private static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // Convert the byte array to hexadecimal string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
