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
using Infoveave.Data.Interfaces;
using Infoveave.Helpers;
using Infoveave.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Infoveave.Controllers
{
    /// <summary>
    /// Widget Data Retrieval
    /// </summary>
    [Authorize("Bearer")]
    [Route("api/{version}/Data")]
    public class DataController : BaseController
    {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cacheProvider"></param>
        /// <param name="configuration"></param>
        public DataController(ITenantContext context, CacheProvider.ICacheProvider cacheProvider, IOptions<ApplicationConfiguration> configuration)
            : base(context, cacheProvider: cacheProvider, configuration: configuration)
        {
        }


        /// <summary>
        /// Get Widget Metadata
        /// </summary>
        /// <param name="id">Widget Id</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [Route("Widgets/{id}")]
        [HttpGet]
        public async Task<Widget> GetWidgetById(long id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var widget = await tenantContext.Widgets.GetAll().FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
            return new Widget
            {
                Id = widget.Id,
                Name = widget.Name,
                Description = widget.FullName,
                Data = widget.SavedData,
                DataSourceIds = widget.DataSourceIds,
                ShortCode = widget.ShortCode,
                Type = widget.ItemType,
                IsPublic = widget.IsPublic,
            };
        }


        /// <summary>
        /// Get Widget Data
        /// </summary>
        /// <param name="requestData">Refer to Model on data</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpPost]
        [Route("Widgets/Data")]
        public async Task<WidgetReponse> WidgetData([FromBody]WidgetRequest requestData, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await this.GetWidgetData(CurrentUser.Tenant, requestData, cancellationToken);

        }

        /// <summary>
        /// Internal 
        /// </summary>
        /// <param name="tenant"></param>
        /// <param name="requestData"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async Task<WidgetReponse> GetWidgetData(string tenant, WidgetRequest requestData, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (requestData == null)
            {
                throw new Exception("Invalid Request, Check Request Data");
            }
            var tenantContext = TenantContext.GetTenantRepository(tenant);
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var uniqueMeasureIds = requestData.Measures.Select(m => m.Id).Distinct();
            var allMeasures = await tenantContext.Measures.GetAll().Where(m => uniqueMeasureIds.Contains(m.Id)).ToListAsync(cancellationToken);
            var uniqueDimensions = requestData.Dimensions.Select(d => d.Query).Union(requestData.Filters.Select(f => f.Query)).Distinct();
            var uniquDataSourceIds = allMeasures.Select(m => m.DataSourceId).Distinct();
            var uniqueDataSources = await tenantContext.DataSources.GetAll().Where(d => uniquDataSourceIds.Contains(d.Id)).ToListAsync(cancellationToken);
            var allDimensions = await tenantContext.Dimensions.GetAll().Where(d => uniqueDimensions.Contains(d.Query)).ToListAsync(cancellationToken);
            var userContext = (CurrentUser != null) ? (await tenantContext.Users.GetAll().FirstOrDefaultAsync(u => u.Id == CurrentUser.Id, cancellationToken)).Context : new List<Models.UserContext>();
            //Group Metrics and Dimensions on DataSource
            var groupedData = uniqueDataSources.Select((ds) =>
            {
                var dataSourceDimensions = allDimensions.Where(d => d.DataSourceId == ds.Id && requestData.Dimensions.Select(rd => rd.Query).Contains(d.Query));
                var dataSourceFilters = allDimensions.Where(d => d.DataSourceId == ds.Id && requestData.Filters.Select(rd => rd.Query).Contains(d.Query));
                var adapterToUse = (ds.ServerType == "mondrianDirect") ? "mondrianService" : ds.ServerType;
                return new
                {
                    Adapter = Helpers.Adapters.OlapAdapters[adapterToUse],
                    DataSource = ds,
                    Measures = allMeasures.Where(me => ds.Id == me.DataSourceId),
                    Dimensions = dataSourceDimensions,
                    Filters = dataSourceDimensions,
                    DimensionData = requestData.Dimensions.Where(d => dataSourceDimensions.Select(dd => dd.Query).Contains(d.Query)).ToList(),
                    FilterData = requestData.Filters.Where(d => dataSourceFilters.Select(dd => dd.Query).Contains(d.Query)).ToList()
                };
            });
            var tasks = groupedData.Select(async (data) =>
            {
                var aq = new AnalysisQuery(data.DataSource.Cube);
                    // data.Measures.Where(m => !string.IsNullOrEmpty(m.DataQuery)).ToList().ForEach(m => aq.WithMembers.Add(m.DataQuery));
                    aq.Measures.AddRange(data.Measures.Select(m => m.Query));
                requestData.Dimensions.ForEach((dimension) =>
                {
                    if (dimension.Query == "Date" || data.DimensionData.Select(d => d.Query).Contains(dimension.Query))
                    {
                        var op = (string.IsNullOrEmpty(dimension.Operator)) ? "Exactly" : dimension.Operator;
                        aq.Dimensions.Add(dimension.Query, new KeyValuePair<string, List<string>>(op, dimension.Items));
                    }
                });
                    /* Commented as this used to screw up the order of the Date.
                    data.DimensionData.ForEach(dim =>
                    {
                        aq.Dimensions.Add(dim.Query, dim.Items);
                    });
                    if (requestData.Dimensions.Any(d => d.Query == "Date"))
                    {
                        aq.Dimensions.Add("Date", requestData.Dimensions.First(d => d.Query == "Date").Items);
                    }
                    */
                data.FilterData.ForEach(fil =>
                {
                    aq.Filters.Add(fil.Query, new KeyValuePair<string, List<string>>(fil.Operator, fil.Items));
                });
                if (requestData.Filters.Any(d => d.Query == "Date"))
                {
                    var dateFilter = requestData.Filters.First(d => d.Query == "Date");
                    aq.Filters.Add("Date", new KeyValuePair<string, List<string>>("Exactly", dateFilter.Items));
                }

                    /* User Context Set */
                var contextForDataSource = (userContext == null) ? new List<Models.UserContext>() : userContext.Where(u => u.DataSourceId == data.DataSource.Id).ToList();
                foreach (var context in contextForDataSource)
                {
                        // See if the dimension is already part of the query
                        if (aq.Dimensions.ContainsKey(context.Query))
                    {
                        aq.Dimensions[context.Query] = new KeyValuePair<string, List<string>>("Exactly", context.Items.ToList());
                    }
                    else if (aq.Filters.ContainsKey(context.Query))
                    {
                        aq.Filters[context.Query] = new KeyValuePair<string, List<string>>("Exactly", context.Items.ToList());
                    }
                    else
                    {
                        aq.Filters.Add(context.Query, new KeyValuePair<string, List<string>>("Exactly", context.Items.ToList()));

                    }

                }

                    /* End User Context Set */



                aq.StartDate = requestData.StartDate;
                aq.EndDate = requestData.EndDate;
                aq.CustomDateDimension = requestData.DateQuery;
                aq.RetrieveEmpty = requestData.RetrieveNullValues;
                var olapConnection = new OlapConnection
                {
                    Server = data.DataSource.Server,
                    Database = data.DataSource.AnalysisDataBase,
                    Cube = data.DataSource.Cube,
                    AdditionalData = (data.DataSource.ServerType == "mondrianDirect") ? data.DataSource.ConnectionString : null,
                };
                olapConnection = await OlapAdapterHelpers.TransformConnectionForAdapter(data.DataSource.ServerType, tenant, data.DataSource.Id, olapConnection, Configuration, HttpContext, Request, cancellationToken);
                return await CacheProvider.ExecuteQueryAsync(data.Adapter, olapConnection, aq, cancellationToken);
            });
            var results = await Task.WhenAll(tasks);
            var mergedData = new List<List<dynamic>>();
            var dimensionColumnNames = results[0].Query.Dimensions.Select(d => (d.Key == requestData.DateQuery) ? "Date" : d.Key);
            for (var i = 0; i < results.Length; i++)
            {
                var originalTable = results[i].Result;
                if (originalTable.Count == 0) continue;
                var dateColumn = results[i].Result[0].FirstOrDefault(d => d.Contains(requestData.DateQuery));
                if (dateColumn != null)
                {
                    results[i].Result[0][results[i].Result[0].IndexOf(dateColumn)] = "Date";
                }
                if (i == 0)
                {
                    var array = new List<dynamic>[originalTable.Count];
                    // For the First table Merge the Table Directly
                    originalTable.CopyTo(array);
                    mergedData = array.ToList();
                    if (results.Length > 1)
                    {
                        mergedData[0].Add("InfoveaveTempKey");
                        for (var rowNo = 1; rowNo < originalTable.Count; rowNo++)
                        {
                            string tempKey = "";
                            for (int colIx = 0; colIx < dimensionColumnNames.Count(); colIx++)
                            {
                                tempKey += originalTable[rowNo][colIx] + "|";
                            }
                            originalTable[rowNo].Add(tempKey);

                        }
                    }
                }
                else
                {

                    // From Second Onwards we have to merge based on the Key

                    // First Add all the additional Columns for Measures
                    foreach (var measure in results[i].Query.Measures)
                    {
                        mergedData[0].Add(measure);
                    }

                    // Now Iterate over each row see if the row based on dimension combo exists
                    for (int rowNo = 0; rowNo < results[i].Result.Count; rowNo++)
                    {
                        string tempKey = "";
                        for (int colIx = 0; colIx < dimensionColumnNames.Count(); colIx++)
                        {
                            tempKey += originalTable[rowNo][colIx] + "|";
                        }
                        List<dynamic> foundRow = mergedData.FirstOrDefault(m => m.Contains(tempKey));
                        if (foundRow != null)
                        {
                            // If Row is found we merge the value to the row
                            foreach (var measure in results[i].Query.Measures)
                            {
                                foundRow.Add(results[i].Result[rowNo][results[i].Result[0].IndexOf(measure)]);
                            }
                        }
                        else
                        {
                            // Else Create a new Row
                            List<dynamic> newRow = new List<dynamic>();

                            // Add all Dimensions
                            for (int dimIx = 0; dimIx < results[i].Query.Dimensions.Count; dimIx++)
                            {
                                newRow.Add(results[i].Result[rowNo][dimIx]);
                            }

                            //Add Key Column
                            newRow.Add(tempKey);
                            var measureCount = 0;
                            // Add Empty Measures for all previous sets
                            for (int ep = 0; ep < i; ep++)
                            {
                                measureCount = +results[ep].Query.Measures.Count;
                            }
                            for (int mc = 0; mc < measureCount; mc++)
                            {
                                newRow.Add(null);
                            }

                            //Now Add Measures from This set
                            for (int cM = 0; cM < results[i].Query.Measures.Count; cM++)
                            {
                                newRow.Add(results[i].Result[rowNo][results[i].Query.Dimensions.Count + cM]);
                            }
                        }

                    }
                }
            }
            if (results.Length > 1)
            {
                var ix = mergedData[0].IndexOf("InfoveaveTempKey");
                for (int i = 0; i < mergedData.Count; i++)
                {
                    mergedData[i].RemoveAt(ix);
                }
            }
            watch.Stop();

            var dataRangeItems = (requestData.Dimensions.FirstOrDefault(r => r.Query == "Date") != null) ? requestData.Dimensions.FirstOrDefault(r => r.Query == "Date").Items : requestData.Filters.FirstOrDefault(r => r.Query == "Date").Items;
            var dateIntervalUsed = (dataRangeItems.Count == 0) ? "" : (dataRangeItems.ElementAt(0).Split('.')[0]);
            return new WidgetReponse()
            {
                Data = mergedData,
                MeasureMetaData = allMeasures.Select(m => new WidgetResponseMeasureMetadata
                {
                    Query = m.Query,
                    IsPercent = false,
                    Prefix = m.Prefix,
                    Suffix = m.Suffix
                }),
                ExecutionTime = watch.Elapsed.Milliseconds,
                DataFetchedFrom = (results.Any(r => r.FromCache == false) ? 0 : 1),
                DateIntervalUsed = dateIntervalUsed
            };

        }


        /// <summary>
        /// Get Widget Data (Anonymous)
        /// </summary>
        /// <param name="tenant">Tenant</param>
        /// <param name="id">Widget Id</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("Widgets/{tenant}/{id}")]
        [Versions("v2")]
        [AllowAnonymous]
        public async Task<Widget> GetWidgetById(string tenant, long id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(tenant);
            if (tenantContext == null)
            {
                return null;
            }
            var widget = await tenantContext.Widgets.GetAll().FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
            if (widget == null)
            {
                return null;
            }
            return new Widget
            {
                Id = widget.Id,
                Name = widget.Name,
                Description = widget.FullName,
                Data = widget.SavedData,
                DataSourceIds = widget.DataSourceIds,
                ShortCode = widget.ShortCode,
                Type = widget.ItemType,
                IsPublic = widget.IsPublic
            };
        }


        /// <summary>
        /// Get Widget Data (Anonymous)
        /// </summary>
        /// <param name="tenant">tenant</param>
        /// <param name="requestData">Refer to Model on data</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpPost("Widgets/{tenant}/Data")]
        [AllowAnonymous]
        public async Task<WidgetReponse> WidgetData(string tenant, [FromBody]WidgetRequest requestData, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await this.GetWidgetData(tenant, requestData, cancellationToken);

        }

        /// <summary>
        /// Delete Widget
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete("Widgets/{id}")]
        [Versions("v2")]
        public async Task<IActionResult> DeleteWidgetById(long id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var widget = await tenantContext.Widgets.GetAll().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
            if (widget.CreatedBy != CurrentUser.Id)
            {
                return Ok();
            }
            var allLinks = await tenantContext.InfoboardItems.GetAll().Where(d => d.WidgetId == id).ToListAsync();
            tenantContext.InfoboardItems.Delete(allLinks);
            tenantContext.Widgets.Delete(widget);
            await tenantContext.CommitAsync();
            return Ok();
        }

        /// <summary>
        /// Toggle Widget Sharing
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut("Widgets/{id}/Share")]
        [Versions("v2")]
        public async Task<IActionResult> ToggleWidgetSharing(long id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var widget = await tenantContext.Widgets.GetAll().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
            if (widget == null) return BadRequest();
            widget.IsPublic = !widget.IsPublic;
            tenantContext.Widgets.Update(widget);
            await tenantContext.CommitAsync();
            return Ok();
        }


        /// <summary>
        /// Add Widget Annotation
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpPost("Widgets/{id}/Annotate")]
        public async Task<WidgetAnnotation> AddWidgetAnnotation(long id, [FromBody]WidgetAnnotation data, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var annotation = new Models.WidgetAnnotation
            {
                WidgetId = id,
                StartDate = data.StartDate,
                EndDate = data.EndDate,
                AnnotationContent = data.Content,
                AnnotationData = data.Data,
                CreatedOn = DateTime.Now,
                CreatedBy = CurrentUser.Id
            };
            var users = await tenantContext.Users.GetAll().ToListAsync();
            tenantContext.WidgetAnnotations.Add(annotation);
            await tenantContext.CommitAsync();
            return new WidgetAnnotation
            {
                Id = annotation.Id,
                StartDate = annotation.StartDate,
                EndDate = annotation.EndDate,
                Content = annotation.AnnotationContent,
                Data = annotation.AnnotationData,
                CreatedBy = annotation.CreatedBy,
                CreatedOn = annotation.CreatedOn,
                CreatedByUser = (users.Any(u => u.Id == annotation.CreatedBy)) ? users.First(u => u.Id == annotation.CreatedBy).FirstName + " " + users.First(u => u.Id == annotation.CreatedBy).LastName : ""
            };
        }


        /// <summary>
        /// Get Widget Annotations
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpPost("Widgets/{id}/Annotations")]
        public async Task<List<WidgetAnnotation>> GetAnnotations(long id, [FromBody]WidgetAnnotation data, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var users = await tenantContext.Users.GetAll().ToListAsync();
            return (await tenantContext.WidgetAnnotations.GetAll().Where(
                a => a.WidgetId == id
                && a.StartDate == data.StartDate
                && a.EndDate == data.EndDate
            ).ToListAsync()).Select(a => new WidgetAnnotation
            {
                Id = a.Id,
                StartDate = a.StartDate,
                EndDate = a.EndDate,
                Data = a.AnnotationData,
                Content = a.AnnotationContent,
                CreatedBy = a.CreatedBy,
                CreatedOn = a.CreatedOn,
                CreatedByUser = (users.Any(u => u.Id == a.CreatedBy)) ? users.First(u => u.Id == a.CreatedBy).FirstName + " " + users.First(u => u.Id == a.CreatedBy).LastName : ""
            }).ToList();
        }

        /// <summary>
        /// Delete Widget Annotation
        /// </summary>
        /// <param name="id"></param>
        /// <param name="annotationId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpDelete("Widgets/{id}/Annotations/{annotationId}")]
        public async Task<IActionResult> AddWidgetAnnotation(long id, long annotationId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var annotation = tenantContext.WidgetAnnotations.GetAll().FirstOrDefault(a => a.Id == annotationId);
            if (annotation == null)
            {
                return BadRequest();
            }
            tenantContext.WidgetAnnotations.Delete(annotation);
            await tenantContext.CommitAsync();
            return Ok();
        }

    }
}
