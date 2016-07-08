using Infoveave.AdapterFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Common;
using Npgsql;
using Microsoft.Extensions.Logging;

namespace Infoveave.Adapters.Postgres
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class SQLAdapter : ISQLAdapter
    {

        private string GetConnectionString(SQLConnection connectionStringObject)
        {
            return $"Server={connectionStringObject.Server};Port={connectionStringObject.Port};Database={connectionStringObject.Database};User Id={connectionStringObject.Username};Password={connectionStringObject.Password};";
        }

        private readonly ILogger logger;
        public SQLAdapter(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger("Adapters:SQL:PGSQL");
        }


        public string Identifier { get { return "pgsql"; } }

        public async Task<bool> CreateTableAsync(SQLConnection connectionString, string tableName, List<ColumnInfo> column)
        {
            using (var connection = new NpgsqlConnection(GetConnectionString(connectionString)))
            {
                await connection.OpenAsync();
                var command = new NpgsqlCommand("",connection);
                await command.ExecuteNonQueryAsync();
                connection.Close();
                return true;
            }
        }

        public async Task DeleteTable(SQLConnection connectionString, string tableName)
        {
            using (var connection = new NpgsqlConnection(GetConnectionString(connectionString)))
            {
                await connection.OpenAsync();
                var command = new NpgsqlCommand($"Delete TABLE IF EXISTS {tableName};",connection);
                await command.ExecuteNonQueryAsync();
                connection.Close();
            }
        }

        public async Task DeleteUploadBatchAsync(SQLConnection connectionString, string tableName, long batchId)
        {
            using (var connection = new NpgsqlConnection(GetConnectionString(connectionString)))
            {
                await connection.OpenAsync();
                var command = new NpgsqlCommand($"DELETE FROM {tableName} Where InfoveaveBatchId={batchId};", connection);
                await command.ExecuteNonQueryAsync();
                connection.Close();
            }
        }

        public async Task<List<Dictionary<string, dynamic>>> ExecuteQueryAsync(SQLConnection connectionString, string query)
        {
            using (var connection = new NpgsqlConnection(GetConnectionString(connectionString)))
            {
                await connection.OpenAsync();
                var command = new NpgsqlCommand(query, connection);
                var reader = await command.ExecuteReaderAsync();
                var results = new List<Dictionary<string, dynamic>>();
                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, dynamic>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row.Add(reader.GetName(i), reader.GetValue(i));
                    }
                    results.Add(row);
                }
                connection.Close();
                return results;
            }
        }

        public async Task<Dictionary<string, string>> GetColumnsAsync(SQLConnection connectionString, string table)
        {
            using (var connection = new NpgsqlConnection(GetConnectionString(connectionString)))
            {
                await connection.OpenAsync();
                var columns = new Dictionary<string,string>();
                var command = new NpgsqlCommand($"SELECT column_name,data_type FROM information_schema.columns WHERE table_schema='public' AND table_name='{table}'", connection);
                var dbReader = await command.ExecuteReaderAsync();
                while (await dbReader.ReadAsync())
                {
                    if (!columns.ContainsKey(dbReader.GetValue(0).ToString()))
                    {
                        var chars = dbReader.GetValue(1).ToString().ToCharArray();
                        chars[0] = chars[0].ToString().ToUpper().ToCharArray()[0];
                        columns.Add(dbReader.GetValue(0).ToString(),string.Join("",chars));
                    }
                }
                connection.Close();
                return columns;
            }

            
        }

        public Task<List<string>> GetDatabasesAsync(SQLConnection connectionString)
        {
            throw new NotImplementedException();
        }

        public KeyValuePair<DbConnection, DbCommand> GetDBConnection(SQLConnection connectionString, string query)
        {
            var connection = new NpgsqlConnection(GetConnectionString(connectionString));
            var command = new NpgsqlCommand(query, connection);
            return new KeyValuePair<DbConnection, DbCommand>(connection, command);
        }

        public async Task<List<string>> GetTablesAsync(SQLConnection connectionString)
        {
            using (var connection = new NpgsqlConnection(GetConnectionString(connectionString)))
            {
                await connection.OpenAsync();
                var tableNames = new List<string>();
                var command = new NpgsqlCommand($"SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'AND table_type = 'BASE TABLE';", connection);
                var dbReader = await command.ExecuteReaderAsync();
                while(await dbReader.ReadAsync())
                {
                    tableNames.Add(dbReader.GetValue(0).ToString());
                }
                connection.Close();
                return tableNames;
            }
        }

        public Task<Dictionary<long, long>> GetUploadBatchInfo(SQLConnection connectionString, string tableName)
        {
            throw new NotImplementedException();
        }

        public Task<bool> InsertDataAsync(SQLConnection connectionString, string tableName, List<List<dynamic>> table)
        {
            throw new NotImplementedException();
        }

        public SQLConnection ParseConnectionString(string connectionString, string tenant)
        {
            throw new NotImplementedException();
        }

        public async Task TruncateTableAsync(SQLConnection connectionString, string tableName)
        {
            using (var connection = new NpgsqlConnection(GetConnectionString(connectionString)))
            {
                await connection.OpenAsync();
                var command = new NpgsqlCommand($"TRUNCATE TABLE {tableName};", connection);
                await command.ExecuteNonQueryAsync();
                connection.Close();
            }
        }
    }
}
