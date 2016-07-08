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
using System.IO;
using Newtonsoft.Json;
using Infoveave.AdapterFramework;
using System.Data;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

#if NET461
#pragma warning disable CS1591 
namespace Infoveave.Helpers
{
    public class ReportMailer
    {
        public static Data.Interfaces.ITenantContext TenantContext;
        public static Infoveave.Mailer.IMailer Mailer;
        public static ILoggerFactory LoggerFactory;
        public static string ReportsPath;
        public ReportMailer()
        {

        }
        public async Task SendReport(string baseURL, ApplicationConfiguration configuration, string tenant, long reportId, List<string> recipients,List<ViewModels.JsonSQLWhereField> parameters)
        {
            var logger = LoggerFactory.CreateLogger("Office Document Updater");
            var tenantContext = TenantContext.GetTenantRepository(tenant);
            var report = await tenantContext.Reports.GetAll().FirstOrDefaultAsync(r => r.Id == reportId);
            if (report == null) return;
            var extension = report.FileName.Split('.').Last();
            string updatedFile = await Documents.CommonDocumentUpdater.UpdateDocument(baseURL, Path.Combine(ReportsPath,tenant, report.FileName), $"{report.Name}-{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}.{extension}");
            if ((extension == "xlsx" || extension == "xlsm") && parameters.Count > 0 && report.ReportId != 0)
            {
                //In this case we know that the report has InfoveaveDataSheet Use that to update
                var dataReport = await tenantContext.DataReports.GetAll().FirstOrDefaultAsync(dr => dr.Id == report.ReportId);
               
                if (dataReport != null) {
                    var dataSource = await tenantContext.DataSources.GetAll().FirstOrDefaultAsync(d => d.Id == dataReport.DataSourceId);
                    if (dataSource != null)
                    {
                        var dbConnection = await GetDataReader(tenant, configuration, dataReport, dataSource, parameters);
                        var result = new List<List<string>>();
                        dbConnection.Key.Open();
                        IDataReader reader = dbConnection.Value.ExecuteReader();
                        int columns = reader.FieldCount;
                        var headerRow = new List<string>();
                        for (int k = 0; k < columns; k++)
                        {
                            headerRow.Add(reader.GetName(k).ToString());
                        }
                        result.Add(headerRow);
                        while (reader.Read())
                        {
                            var row = new List<string>();
                            for (int k = 0; k < columns; k++)
                            {
                                row.Add(reader.GetValue(k).ToString());
                            }
                            result.Add(row);
                        }
                        dbConnection.Key.Close();
                        Documents.ExcelQueryReportUpdater.UpdateDocument(logger, updatedFile, result);
                    }
                    
                }
            }
            var message = Mailer.MergeTemplate("en", "Report", new Dictionary<string, string>
                {
                    { "Domain",tenant },
                });
            await Mailer.SendMail(
                    recipients.Select(r => new Infoveave.Mailer.Recipient { Email = r }).ToList(),
                    $" Infoveave Updated Report : {report.Name}",
                    message,
                    attachments: new List<FileInfo> { new FileInfo(updatedFile) }
            );
        }

        public void SendReportLater(string baseURL, ApplicationConfiguration configuration, string tenant, long reportId, List<string> recipients, List<ViewModels.JsonSQLWhereField> parameters)
        {
            Task.Run(async () =>
            {
                await SendReport(baseURL, configuration, tenant, reportId, recipients, parameters);
            }).Wait();
        }

        public async Task<KeyValuePair<System.Data.Common.DbConnection, System.Data.Common.DbCommand>> GetDataReader(string tenant, ApplicationConfiguration configuration, Models.DataReports report, Models.DataSource dataSource, List<ViewModels.JsonSQLWhereField> parameters)
        {
            var sqlConnection = JsonConvert.DeserializeObject<SQLConnection>(dataSource.ConnectionString);
            sqlConnection = await OlapAdapterHelpers.TransformSQLConnection(sqlConnection, tenant, configuration);
            var baseQuery = JsonConvert.DeserializeObject<ViewModels.JsonSQLQuery>(report.DataStructure);
            baseQuery.Parameters = parameters;
            var sqlQuery = JsonToSQL.GenerateSQL(baseQuery, dataSource.TableName, dataSource.ServerType);
            Debug.WriteLine(sqlQuery);
            var sqlAdapter = Helpers.Adapters.SQLAdapters[sqlConnection.Type];
            var dbConnection = sqlAdapter.GetDBConnection(sqlConnection, sqlQuery);
            return dbConnection;
        }

        public async Task SendDataReport(string tenant, ApplicationConfiguration configuration, long reportId, List<string> recipients, List<ViewModels.JsonSQLWhereField> parameters)
        {
            var tenantContext = TenantContext.GetTenantRepository(tenant);
            var report = await tenantContext.DataReports.GetAll().FirstOrDefaultAsync(r => r.Id == reportId);
            if (report == null) return;
            var dataSource = await tenantContext.DataSources.GetAll().FirstOrDefaultAsync(d => d.Id == report.DataSourceId);
            if (dataSource == null) return;
            var dbConnection = await GetDataReader(tenant, configuration, report, dataSource, parameters);
            var updatedFile = Path.GetTempFileName();
            updatedFile = updatedFile.Replace(updatedFile.Split('\\').Last(), $"{report.Name}-{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}.csv");
            using (var sw = new StreamWriter(updatedFile))
            {
                dbConnection.Key.Open();
                IDataReader reader = dbConnection.Value.ExecuteReader();
                int columns = reader.FieldCount;
                string column = "";
                for (int k = 0; k < columns; k++)
                {
                    if (k == columns - 1)
                        column += "\"" + reader.GetName(k) + "\"";
                    else
                        column += "\"" + reader.GetName(k) + "\",";
                }
                sw.WriteLine(column);
                while (reader.Read())
                {
                    var row = string.Empty;
                    for (int k = 0; k < columns; k++)
                    {
                        if (k == columns - 1)
                            row += "\"" + reader.GetValue(k) + "\"";
                        else
                            row += "\"" + reader.GetValue(k) + "\",";
                    }
                    sw.WriteLine(row);
                }
                dbConnection.Key.Close();
                sw.Close();
            }
            var message = Mailer.MergeTemplate("en", "Report", new Dictionary<string, string>
                {
                    { "Domain",tenant },
                });
            await Mailer.SendMail(
                    recipients.Select(r => new Infoveave.Mailer.Recipient { Email = r }).ToList(),
                    $" Infoveave Updated Report : {report.Name}",
                    message,
                    attachments: new List<FileInfo> { new FileInfo(updatedFile) }
            );
        }

        public void SendDataReportLater(string tenant, ApplicationConfiguration configuration, long reportId, List<string> recipients, List<ViewModels.JsonSQLWhereField> parameters)
        {
            Task.Run(async () =>
            {
                await SendDataReport(tenant, configuration, reportId, recipients, parameters);
            }).Wait();
        }

    }
}
#pragma warning restore CS1591
#endif
