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
using System.Linq;
using System.Threading.Tasks;
using Infoveave.AdapterFramework;
using System.Globalization;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;

namespace Infoveave.Adapters.Sql
{
    public class SQLAdapter : ISQLAdapter
    {
        public string Identifier { get { return "Sql"; } }
        private readonly ILogger logger;
        public SQLAdapter(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger("Adapters:SQL:MicrosoftSQL");
        }

        private IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                yield return day;
        }
        private static int GetQuarter(DateTime date)
        {
            int[] quarters = { 1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 4 };
            return quarters[date.Month - 1];
        }
        private static int GetSemester(DateTime date)
        {
            if (date.Month > 6) { return 2; }
            else return 1;

        }

        public Task<bool> CreateTableAsync(SQLConnection connectionString, string tableName, List<ColumnInfo> column)
        {



            using (var connection = new SqlConnection(GetConnectionString(connectionString)))
            {
                connection.Open();
                string existsQuery = "Select count(*) from dimdate;";
                var exCommand = new SqlCommand(existsQuery, connection);
                try
                {
                    var reader = exCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        // Do Nothing
                    };
                }
                catch (Exception)
                {

                    exCommand.CommandText = @"CREATE TABLE dimdate ( Date int , Year int , YearName nvarchar(50) , SemesterNo int , Semester int , SemesterName nvarchar(50) , QuarterNo int , Quarter int , QuarterName nvarchar(50) , MonthNo int , Month int , MonthName nvarchar(50) , WeekNo int , Week int , WeekName nvarchar(50), FormattedDate nvarchar(10));";
                    exCommand.ExecuteNonQuery();
                    exCommand.CommandText = "Insert Into dimdate (Date, Year, YearName, SemesterNo, Semester, SemesterName, QuarterNo, Quarter, QuarterName, MonthNo, Month, MonthName, WeekNo, Week, WeekName, FormattedDate) Values " +
                            "(@Date, @Year, @YearName, @SemesterNo, @Semester, @SemesterName, @QuarterNo, @Quarter, @QuarterName, @MonthNo, @Month, @MonthName, @WeekNo, @Week, @WeekName, @FormattedDate)";
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        var calendar = DateTimeFormatInfo.CurrentInfo.Calendar;
                        foreach (var dateTime in EachDay(new DateTime(1990, 1, 1), new DateTime(2020, 12, 31)))
                        {
                            exCommand.Transaction = transaction;
                            exCommand.Parameters.Clear();
                            exCommand.Parameters.AddWithValue("@Date", Convert.ToInt32(dateTime.ToString("yyyyMMdd")));
                            exCommand.Parameters.AddWithValue("@Year", (int)dateTime.Year);
                            exCommand.Parameters.AddWithValue("@YearName", string.Format("Calendar {0}", dateTime.Year));
                            exCommand.Parameters.AddWithValue("@SemesterNo", (int)GetSemester(dateTime));
                            exCommand.Parameters.AddWithValue("@Semester", Convert.ToInt32(string.Format("{0}{1}", dateTime.Year, GetSemester(dateTime))));
                            exCommand.Parameters.AddWithValue("@SemesterName", string.Format("Semester {0}, {1}", GetSemester(dateTime), dateTime.Year));
                            exCommand.Parameters.AddWithValue("@QuarterNo", (int)GetQuarter(dateTime));
                            exCommand.Parameters.AddWithValue("@Quarter", Convert.ToInt32(string.Format("{0}{1}", dateTime.Year, GetQuarter(dateTime))));
                            exCommand.Parameters.AddWithValue("@QuarterName", string.Format("Quarter {0}, {1}", GetQuarter(dateTime), dateTime.Year));
                            exCommand.Parameters.AddWithValue("@MonthNo", (int)calendar.GetMonth(dateTime));
                            exCommand.Parameters.AddWithValue("@Month", Convert.ToInt32(string.Format("{0}{1}", dateTime.Year, String.Format("{0:00}", calendar.GetMonth(dateTime)))));
                            exCommand.Parameters.AddWithValue("@MonthName", string.Format("{0} {1}", dateTime.ToString("MMMM"), dateTime.Year));
                            exCommand.Parameters.AddWithValue("@WeekNo", (int)calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstDay, DayOfWeek.Monday));
                            exCommand.Parameters.AddWithValue("@Week", Convert.ToInt32(string.Format("{0}{1}", dateTime.Year, String.Format("{0:00}", calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstDay, DayOfWeek.Monday)))));
                            exCommand.Parameters.AddWithValue("@WeekName", string.Format("Week {0}, {1}", calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstDay, DayOfWeek.Monday), dateTime.Year));
                            exCommand.Parameters.AddWithValue("@FormattedDate", dateTime.ToString("yyyy-MM-dd"));
                            exCommand.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                }
                string tableQuery = "CREATE TABLE " + tableName + "(";
                foreach (var item in column)
                {
                    tableQuery += " " + item.ColumnName + " ";
                    string datatype = string.Empty;
                    switch (item.DataType)
                    {
                        case "Decimal":
                            datatype = "decimal (24, 4)";
                            break;
                        case "Number":
                            datatype = "bigint";
                            break;
                        case "Integer":
                            datatype = "bigint";
                            break;
                        case "Text":
                            datatype = "nvarchar(100)";
                            break;
                        case "Date":
                            datatype = "date";
                            break;
                        case "Boolean":
                            datatype = "bit";
                            break;
                        default:
                            datatype = "nvarchar (100)";
                            break;
                    }
                    tableQuery += datatype + " , ";
                }
                tableQuery = tableQuery.Remove(tableQuery.LastIndexOf(","));
                tableQuery += ");";
                SqlCommand command = new SqlCommand(tableQuery, connection);

                command.ExecuteNonQuery();
                connection.Close();
                return Task.FromResult(true);
            }
        }

        public async Task<List<Dictionary<string, dynamic>>> ExecuteQueryAsync(SQLConnection connectionString, string query)
        {
            using (var connection = new SqlConnection(GetConnectionString(connectionString)))
            {
                await connection.OpenAsync();
                var command = new SqlCommand(query, connection);
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

        public Task<Dictionary<string, string>> GetColumnsAsync(SQLConnection connectionString, string table)
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> GetDatabasesAsync(SQLConnection connectionString)
        {
            throw new NotImplementedException();
        }

        public KeyValuePair<System.Data.Common.DbConnection, System.Data.Common.DbCommand> GetDBConnection(SQLConnection connectionString, string query)
        {
            var connection = new SqlConnection(GetConnectionString(connectionString));
            SqlCommand command = connection.CreateCommand();
            command.CommandText = query;
            return new KeyValuePair<System.Data.Common.DbConnection, System.Data.Common.DbCommand>(connection, command);
        }

        public Task<List<string>> GetTablesAsync(SQLConnection connectionString)
        {
            List<string> tableslist = new List<string>();
            using (var connection = new SqlConnection(GetConnectionString(connectionString)))
            {
                SqlCommand command = connection.CreateCommand();
                command.CommandText = "SHOW TABLES;";
                connection.Open();
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                        tableslist.Add(reader.GetValue(i).ToString());
                }
                connection.Close();
                return Task.FromResult(tableslist);
            }
        }

        public async Task DeleteTable(SQLConnection connectionString, string table)
        {
            using (var connection = new SqlConnection(GetConnectionString(connectionString)))
            {
                SqlCommand command = connection.CreateCommand();
                command.CommandText = $"DROP TABLE IF EXISTS {table};";
                connection.Open();
                await command.ExecuteNonQueryAsync();
                connection.Close();
            }
        }

        public Task<bool> InsertDataAsync(SQLConnection connectionString, string tableName, List<List<dynamic>> table)
        {
            using (var connection = new SqlConnection(GetConnectionString(connectionString)))
            {
                connection.Open();
                var columnNames = table[0].Select(t => t.ToString());
                var query = "Insert into " + tableName + "(" + string.Join(", ", columnNames) + ") Values (" + string.Join(", ", columnNames.Select(c => "@" + c)) + ")";
                using (var command = new SqlCommand(query, connection))
                {
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        for (var i = 2; i < table.Count; i++)
                        {
                            command.Transaction = transaction;
                            command.Parameters.Clear();
                            columnNames.ToList().ForEach(c =>
                            {
                                var value = table[i][table[0].IndexOf(c)];
                                if (value == null) value = DBNull.Value;
                                if (value is System.Numerics.BigInteger)
                                {
                                    command.Parameters.AddWithValue("@" + c, ((System.Numerics.BigInteger)value).ToString());
                                }
                                else
                                    command.Parameters.AddWithValue("@" + c, value);
                            });
                            command.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                }
                connection.Close();
                return Task.FromResult(true);
            }
        }
        public SQLConnection ParseConnectionString(string connectionString, string tenant)
        {
            var connectionStringObject = new SQLConnection();
            connectionStringObject.Server = connectionString;
            connectionStringObject.Username = "";
            connectionStringObject.Password = "Infoveave@123";
            connectionStringObject.Type = "Sql";
            connectionStringObject.Database = tenant;
            return connectionStringObject;
        }


        private string GetConnectionString(SQLConnection connectionStringObject)
        {
            var filePath = System.IO.Path.Combine(connectionStringObject.Server, connectionStringObject.Database + ".db");
            return string.Format($"Data Source={filePath};", connectionStringObject.Server, connectionStringObject.Database);
        }



        public async Task DeleteUploadBatchAsync(SQLConnection connectionString, string tableName, long batchId)
        {
            using (var connection = new SqlConnection(GetConnectionString(connectionString)))
            {
                SqlCommand command = connection.CreateCommand();
                command.CommandText = $"DELETE FROM {tableName} Where InfoveaveBatchId={batchId};";
                connection.Open();
                await command.ExecuteNonQueryAsync();
                connection.Close();
            }
        }
        public async Task TruncateTableAsync(SQLConnection connectionString, string tableName)
        {
            using (var connection = new SqlConnection(GetConnectionString(connectionString)))
            {
                SqlCommand command = connection.CreateCommand();
                command.CommandText = $"TRUNCATE TABLE {tableName};";
                connection.Open();
                await command.ExecuteNonQueryAsync();
                connection.Close();
            }
        }

        public async Task<Dictionary<long,long>> GetUploadBatchInfo(SQLConnection connectionString, string tableName)
        {
            using (var connection = new SqlConnection(GetConnectionString(connectionString)))
            {
                SqlCommand command = connection.CreateCommand();
                command.CommandText = $"Select [InfoveaveBatchId], Count(*) from {tableName} group by [InfoveaveBatchId];";
                connection.Open();
                var reader = await command.ExecuteReaderAsync();
                var res = new Dictionary<long, long>();
                while (await reader.ReadAsync())
                {
                    res.Add((long)reader.GetValue(0), (long)reader.GetValue(1));
                }
                connection.Close();
                return res;
            }
        }
    }
}
