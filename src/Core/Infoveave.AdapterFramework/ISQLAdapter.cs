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
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infoveave.AdapterFramework
{
    public interface ISQLAdapter
    {
        string Identifier { get;}
        Task<List<string>> GetTablesAsync(SQLConnection connectionString);
        Task<List<Dictionary<string,dynamic>>> ExecuteQueryAsync(SQLConnection connectionString, string query);
        Task<Dictionary<string, string>> GetColumnsAsync(SQLConnection connectionString, string table);
        KeyValuePair<System.Data.Common.DbConnection, System.Data.Common.DbCommand> GetDBConnection(SQLConnection connectionString, string query);
        Task<List<string>> GetDatabasesAsync(SQLConnection connectionString);
        SQLConnection ParseConnectionString(string connectionString, string tenant);
        Task<bool> CreateTableAsync(SQLConnection connectionString, string tableName, List<ColumnInfo> column);
        Task<bool> InsertDataAsync(SQLConnection connectionString, string tableName, List<List<dynamic>> table);
        Task DeleteTable(SQLConnection connectionString, string tableName);
        Task DeleteUploadBatchAsync(SQLConnection connectionString, string tableName, long batchId);
        Task TruncateTableAsync(SQLConnection connectionString, string tableName);
        Task<Dictionary<long, long>> GetUploadBatchInfo(SQLConnection connectionString, string tableName);

    }

    public class SQLConnection
    {
        public string Server { get; set; }
        public string Type { get; set; }
        public string Database { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public long Port { get; set; }
    }

    public class ColumnInfo
    {
        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public int ColumnSize { get; set; }
        public bool IsDimension { get; set; }
    }


    public class AdapterWithConnection
    {
        public ISQLAdapter adpter { get; set; }
        public SQLConnection ConnectionStringObject { get; set; }
    }

}
