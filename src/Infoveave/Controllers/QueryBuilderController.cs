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
using Infoveave.Data.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Infoveave.ViewModels;
using Infoveave.Helpers;
using System;
using Newtonsoft.Json;
using System.Data;
using System.IO;
using Infoveave.AdapterFramework;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Infoveave.Controllers
{
    /// <summary>
    /// Query Builder Controller
    /// </summary>
    [Authorize]
    [Route("api/{version}/QueryBuilder")]
    public class QueryBuilderController : BaseController
    {
        private Mailer.IMailer Mailer { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        /// <param name="cacheProvider"></param>
        /// <param name="mailer"></param>
        public QueryBuilderController(ITenantContext context, IOptions<ApplicationConfiguration> configuration, CacheProvider.ICacheProvider cacheProvider, Mailer.IMailer mailer)
            : base(context, configuration: configuration, cacheProvider: cacheProvider)
        {
            Mailer = mailer;
        }

        /// <summary>
        /// Get Data Reports
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpGet("")]
        public async Task<List<Report>> GetDataReports(CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var datareports = await tenantContext.DataReports.GetAll().ToListAsync(cancellationToken);
            var Reports = datareports.Select(s =>
            {
                var parameter = new dynamic[0];
                var scheduleParameter = new dynamic[0];
                if (!string.IsNullOrEmpty(s.Parameter))
                {
                    parameter = JsonConvert.DeserializeObject<dynamic[]>(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<List<JsonSQLWhereField>>(s.Parameter), new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() }));
                }
                else
                {
                    parameter = new string[0];
                }
                if (!string.IsNullOrEmpty(s.ScheduleParameter))
                {
                    scheduleParameter = JsonConvert.DeserializeObject<dynamic[]>(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<List<JsonSQLWhereField>>(s.ScheduleParameter), new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() }));
                }
                else
                {
                    scheduleParameter = new string[0];
                }
                return new Report
                {
                    Id = s.Id,
                    Name = s.Name,
                    CreatedBy = (tenantContext.Users.GetAll().Where(u => u.Id == Convert.ToInt32(s.CreatedBy)).FirstOrDefault() == null) ? "" : tenantContext.Users.GetAll().Where(u => u.Id == Convert.ToInt32(s.CreatedBy)).FirstOrDefault().UserName,
                    CreatedOn = s.CreatedOn,
                    ScheduleReport = s.ScheduleReport,
                    MailTo = s.MailTo,
                    Parameter = parameter,
                    ScheduleParameter = scheduleParameter,
                    DataSourceId = s.DataSourceId,
                };
            }).ToList();
            return Reports;
        }

        /// <summary>
        /// Get DataReport By Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpGet("{id}")]
        public async Task<Report> GetDataReport(long id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var dataReport = await tenantContext.DataReports.GetAll().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
            if (dataReport == null) throw new Exception("QG-0001", new Exception("DataReport Not Found"));
            var parameter = new dynamic[0];
            var scheduleParameter = new dynamic[0];
            if (!string.IsNullOrEmpty(dataReport.Parameter))
            {
                parameter = JsonConvert.DeserializeObject<dynamic[]>(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<List<JsonSQLWhereField>>(dataReport.Parameter), new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() }));
            }
            else
            {
                parameter = new string[0];
            }
            if (!string.IsNullOrEmpty(dataReport.ScheduleParameter))
            {
                scheduleParameter = JsonConvert.DeserializeObject<dynamic[]>(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<List<JsonSQLWhereField>>(dataReport.ScheduleParameter), new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() }));
            }
            else
            {
                scheduleParameter = new string[0];
            }
            return new Report
            {
                Id = dataReport.Id,
                Name = dataReport.Name,
                CreatedOn = dataReport.CreatedOn,
                Parameter = parameter,
                DataSourceId = dataReport.DataSourceId,
                ScheduleParameter = scheduleParameter
            };
        }

        /// <summary>
        /// Delete DataReport
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDataReport(long id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var datareport = await tenantContext.DataReports.GetAll().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
            var reports = tenantContext.Reports.GetAll().Where(d => d.ReportId == datareport.Id).FirstOrDefault();
            if (reports != null)
            {
                return null;
            }
            tenantContext.DataReports.Delete(datareport);
            await tenantContext.CommitAsync(cancellationToken);
            return Ok();
        }

        /// <summary>
        /// Get Data Sources
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpGet("DataSources")]
        public async Task<List<Infoveave.ViewModels.DataSource>> GetDataSources(CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var dataSources = await tenantContext.DataSources.GetAll().Where(d => d.TypeId == 2 || d.TypeId == 80).ToListAsync();
            return dataSources.Select(d => new DataSource
            {
                Id = d.Id,
                Name = d.Name
            }).ToList();
        }

        /// <summary>
        /// Get Tables in a DataSource
        /// </summary>
        /// <param name="id">DataSourceId</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")][HttpGet("DataSources/{id}/Tables")]
        public async Task<List<TableAndColumnMappings>> GetTableNames(long id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var dataSource = await tenantContext.DataSources.GetAll().FirstOrDefaultAsync(d => d.Id== id);
            if (dataSource.TypeId == 2)
            {
                return new List<TableAndColumnMappings>
                {
                    new TableAndColumnMappings
                    {
                        DataSourceId = id,
                        TableName = dataSource.TableName,
                        ColumnMapping = JsonConvert.DeserializeObject<List<ColumnMapping>>(dataSource.ColumnMappings)
                    }
                };
            }
            if (dataSource.TypeId == 80)
            {
                var sqlConnection = JsonConvert.DeserializeObject<SQLConnection>(dataSource.Server);
                if (sqlConnection.Type == "mysql") return new List<TableAndColumnMappings>();
                var tables = await Helpers.Adapters.SQLAdapters[sqlConnection.Type].GetTablesAsync(sqlConnection);
                var list = tables.Select(async (table) =>
                {
                    var columns = await Helpers.Adapters.SQLAdapters[sqlConnection.Type].GetColumnsAsync(sqlConnection, table);
                    return new TableAndColumnMappings()
                    {
                        TableName = table,
                        DataSourceId = id,
                        ColumnMapping = columns.Select(c => new ColumnMapping
                        {
                            IsComputed = false,
                            SourceName = c.Key,
                            DisplayName = c.Key,
                            TableName = c.Key,
                            IsMandatory = false,
                            Format = null,
                            Type = c.Value
                        }).ToList()
                    };
                });
                var results = await Task.WhenAll(list);
                return results.ToList();
            }
            return new List<TableAndColumnMappings>();
        }


        /// <summary>
        /// Get Field Items
        /// </summary>
        /// <param name="dataSourceId"></param>
        /// <param name="columnName"></param>
        /// <param name="tableName"></param>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpGet("Field/{dataSourceId}/{columnName}/{tableName}")]
        public async Task<List<string>> GetUniqueFieldItems(long dataSourceId, string columnName, string tableName, string query, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var dataSource = await tenantContext.DataSources.GetAll().FirstOrDefaultAsync(d => d.Id == dataSourceId);
            var sqlConnection = JsonConvert.DeserializeObject<SQLConnection>(dataSource.Server);
            sqlConnection = await OlapAdapterHelpers.TransformSQLConnection(sqlConnection, CurrentUser.Tenant, Configuration);
            var sqlAdapter = Helpers.Adapters.SQLAdapters[sqlConnection.Type];
            var sqlquery = $"select distinct {JsonToSQL.WrapField(columnName, sqlConnection.Type)} from {tableName ?? dataSource.TableName} where Lower({JsonToSQL.WrapField(columnName, sqlConnection.Type)}) like '%{query.ToLower()}%'";
            var connectionReader = sqlAdapter.GetDBConnection(sqlConnection, sqlquery);
            connectionReader.Key.Open();
            var reader = connectionReader.Value.ExecuteReader();
            List<string> items = new List<string>();
            while (reader.Read())
            {
                items.Add(reader.GetValue(0).ToString());
            }
            return items;
        }

        /// <summary>
        /// Preview with SQL and Data
        /// </summary>
        /// <param name="dataSourceId"></param>
        /// <param name="tableName"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpPost("{dataSourceId}/{tableName}/SQLPreview")]
        public async Task<SQLAndPreviewResult> GetSQLQueryAndPreview(long dataSourceId, string tableName, [FromBody]JsonSQLQuery query)
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var dataSource = await tenantContext.DataSources.GetAll().FirstOrDefaultAsync(d => d.Id == dataSourceId);
            var sqlConnection = JsonConvert.DeserializeObject<SQLConnection>(dataSource.Server);
            sqlConnection = await OlapAdapterHelpers.TransformSQLConnection(sqlConnection, CurrentUser.Tenant, Configuration);
            var sqlQuery = JsonToSQL.GenerateSQL(query, tableName ?? dataSource.TableName, sqlConnection.Type);

            var adapter = Helpers.Adapters.SQLAdapters[sqlConnection.Type];
            var result = await adapter.ExecuteQueryAsync(sqlConnection, sqlQuery);
            return new SQLAndPreviewResult { Query = sqlQuery, Result = JsonConvert.SerializeObject(result.Take(50)), TotalRowCount = result.Count };
        }


        /// <summary>
        /// SQL Preview 
        /// </summary>
        /// <param name="dataSourceId"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpPost("{dataSourceId}/ManualSQLPreview")]
        public async Task<SQLAndPreviewResult> GetManualSQLQueryAndPreview(long dataSourceId, [FromBody]ManualSQLQuery query)
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var dataSource = await tenantContext.DataSources.GetAll().FirstOrDefaultAsync(d => d.Id == dataSourceId);
            var sqlConnection = JsonConvert.DeserializeObject<SQLConnection>(dataSource.Server);
            sqlConnection = await OlapAdapterHelpers.TransformSQLConnection(sqlConnection, CurrentUser.Tenant, Configuration);
            var sqlQuery = query.Query;
            foreach (var parameter in query.Parameters)
            {
                var manualWhereParam = JsonToSQL.ManualWhereFieldBuilder(parameter, query.Parameters, sqlConnection.Type);
                sqlQuery = sqlQuery.Replace("@[" + parameter.Field.Value + "](columnValue:__id__)", manualWhereParam);
            }
            sqlQuery = sqlQuery.Replace("(columnName:__id__)", "").Replace("(table:__id__)", "").Replace("(columnValue:__id__)", "");
            sqlQuery = sqlQuery.Replace("@[" + dataSource.Name + "]", dataSource.TableName).Replace("@", "");
            var adapter = Helpers.Adapters.SQLAdapters[sqlConnection.Type];
            var result = await adapter.ExecuteQueryAsync(sqlConnection, sqlQuery);
            return new SQLAndPreviewResult { Query = sqlQuery, Result = JsonConvert.SerializeObject(result.Take(50)), TotalRowCount = result.Count };
        }

        /// <summary>
        /// Save Manual SQL 
        /// </summary>
        /// <param name="report"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpPost("SaveManualSQL")]
        public async Task<DataReport> AddManualQueryReport([FromBody] DataReport report, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            if (report.Name == null)
            {
                throw new Exception("AUTH-Q001");
            }
            var existingDataReport = await tenantContext.DataReports.GetAll().Where(r => r.Name == report.Name).ToListAsync(cancellationToken);
            if (existingDataReport.FirstOrDefault() != null)
            {
                throw new Exception("AUTH-Q002");
            }
            var dataSource = await tenantContext.DataSources.GetAll().FirstOrDefaultAsync(d => d.Id == report.DataSourceId);
            var sqlConnection = JsonConvert.DeserializeObject<SQLConnection>(dataSource.Server);
            sqlConnection = await OlapAdapterHelpers.TransformSQLConnection(sqlConnection, CurrentUser.Tenant, Configuration);
            Models.DataReports dataReport = new Models.DataReports
            {
                Name = report.Name,
                CreatedBy = CurrentUser.Id,
                DataStructure = JsonConvert.SerializeObject(report.JsonSQLQuery),
                CreatedOn = DateTime.Now,
                Query = report.Query.Replace("(columnName:__id__)", "").Replace("(table:__id__)", ""),
                Parameter = JsonConvert.SerializeObject(report.JsonSQLQuery.Parameters),
                ConnectionString = (dataSource == null) ? "" : dataSource.Server,
                DataSourceId = report.DataSourceId
            };
            dataReport.Query = dataReport.Query.Replace("@[" + dataSource.Name + "]", dataSource.TableName);
            tenantContext.DataReports.Add(dataReport);
            await tenantContext.CommitAsync(cancellationToken);
            report.Id = dataReport.Id;
            return report;
        }

        /// <summary>
        /// Save SQL
        /// </summary>
        /// <param name="report"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpPost("SaveSQL")]
        public async Task<DataReport> AddReport([FromBody] DataReport report, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            if (report.Name == null)
            {
                throw new Exception("AUTH-Q001");
            }
            var existingDataReport = await tenantContext.DataReports.GetAll().Where(r => r.Name == report.Name).ToListAsync(cancellationToken);
            if (existingDataReport.FirstOrDefault() != null)
            {
                throw new Exception("AUTH-Q002");
            }
            var dataSource = await tenantContext.DataSources.GetAll().FirstOrDefaultAsync(d => d.Id == report.DataSourceId);
            var sqlConnection = JsonConvert.DeserializeObject<SQLConnection>(dataSource.Server);
            sqlConnection = await OlapAdapterHelpers.TransformSQLConnection(sqlConnection, CurrentUser.Tenant, Configuration);
            Models.DataReports dataReport = new Models.DataReports
            {
                Name = report.Name,
                CreatedBy = CurrentUser.Id,
                DataStructure = JsonConvert.SerializeObject(report.JsonSQLQuery),
                CreatedOn = DateTime.Now,
                Query = report.Query,
                Parameter = JsonConvert.SerializeObject(report.JsonSQLQuery.Parameters),
                ConnectionString = (dataSource == null) ? "" : dataSource.Server,
                DataSourceId = report.DataSourceId
            };
            tenantContext.DataReports.Add(dataReport);
            await tenantContext.CommitAsync(cancellationToken);
            report.Id = dataReport.Id;
            return report;
        }

        /// <summary>
        /// Update SQL
        /// </summary>
        /// <param name="id"></param>
        /// <param name="report"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpPost("{id}/SaveSQL")]
        public async Task<DataReport> UpdateReport(int id, [FromBody] DataReport report, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            if (report.Name == null)
            {
                throw new Exception("AUTH-Q001");
            }
            var existingDataReport = await tenantContext.DataReports.GetAll().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
            if (existingDataReport == null)
            {
                throw new Exception("AUTH-Q002");
            }
            var dataSource = await tenantContext.DataSources.GetAll().FirstOrDefaultAsync(d => d.Id == report.DataSourceId);
            var sqlConnection = JsonConvert.DeserializeObject<SQLConnection>(dataSource.Server);
            sqlConnection = await OlapAdapterHelpers.TransformSQLConnection(sqlConnection, CurrentUser.Tenant, Configuration);
            var dataReport = existingDataReport;
            dataReport.Name = report.Name;
            dataReport.CreatedBy = CurrentUser.Id;
            dataReport.DataStructure = JsonConvert.SerializeObject(report.JsonSQLQuery);
            dataReport.CreatedOn = DateTime.Now;
            dataReport.Query = report.Query;
            dataReport.Parameter = JsonConvert.SerializeObject(report.JsonSQLQuery.Parameters);
            dataReport.ConnectionString = (dataSource == null) ? "" : dataSource.Server;
            dataReport.DataSourceId = report.DataSourceId;
            tenantContext.DataReports.Add(dataReport);
            tenantContext.DataReports.Update(dataReport);
            await tenantContext.CommitAsync(cancellationToken);
            report.Id = dataReport.Id;
            return report;
        }

        /// <summary>
        /// Update Manual SQL Query
        /// </summary>
        /// <param name="id"></param>
        /// <param name="report"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpPost("{id}/SaveManualSQL")]
        public async Task<DataReport> UpdateManualQueryReport(int id, [FromBody] DataReport report, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            if (report.Name == null)
            {
                throw new Exception("AUTH-Q001");
            }
            var existingDataReport = await tenantContext.DataReports.GetAll().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
            if (existingDataReport == null)
            {
                throw new Exception("AUTH-Q002");
            }
            var dataSource = await tenantContext.DataSources.GetAll().FirstOrDefaultAsync(d => d.Id == report.DataSourceId);
            var sqlConnection = JsonConvert.DeserializeObject<SQLConnection>(dataSource.Server);
            sqlConnection = await OlapAdapterHelpers.TransformSQLConnection(sqlConnection, CurrentUser.Tenant, Configuration);
            var dataReport = existingDataReport;
            dataReport.Name = report.Name;
            dataReport.CreatedBy = CurrentUser.Id;
            dataReport.DataStructure = JsonConvert.SerializeObject(report.JsonSQLQuery);
            dataReport.CreatedOn = DateTime.Now;
            dataReport.Query = report.Query;
            dataReport.Parameter = JsonConvert.SerializeObject(report.JsonSQLQuery.Parameters);
            dataReport.ConnectionString = (dataSource == null) ? "" : dataSource.Server;
            dataReport.DataSourceId = report.DataSourceId;

            tenantContext.DataReports.Update(dataReport);
            await tenantContext.CommitAsync(cancellationToken);
            report.Id = dataReport.Id;
            return report;
        }

    }

}
