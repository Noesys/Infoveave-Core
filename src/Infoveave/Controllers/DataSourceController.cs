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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;


namespace Infoveave.Controllers
{

    /// <summary>
    /// DataSource
    /// </summary>
    [Authorize("Bearer")]
    [Route("api/{version}/DataSources")]
    public class DataSourceController : BaseController
    {
        private ILogger logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="cacheProvider"></param>
        public DataSourceController(ITenantContext context, IOptions<ApplicationConfiguration> configuration, ILoggerFactory loggerFactory, CacheProvider.ICacheProvider cacheProvider)
            : base(context, configuration: configuration, cacheProvider: cacheProvider)
        {
            logger = loggerFactory.CreateLogger("DataSourceController");

        }

        /// <summary>
        /// Get DataSources
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpGet("")]
        public async Task<IEnumerable<DataSource>> GetDataSources(CancellationToken cancellationToken = default(CancellationToken))
        {
            var sources = await TenantContext.GetTenantRepository(CurrentUser.Tenant).DataSources.GetAll().Where(d => d.CreatedBy == CurrentUser.Id || d.IsPublic == true).ToListAsync(cancellationToken);
            return sources.Select(d => new DataSource
            {
                Id = d.Id,
                Type = d.ServerType,
                Name = d.Name,
                IsPublic = d.IsPublic,
                CanShare = (d.CreatedBy == CurrentUser.Id)
            });
        }

        /// <summary>
        /// Get DataSource by Id
        /// </summary>
        /// <param name="id">DataSource Id</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpGet("{id}")]
        public async Task<Infoveave.ViewModels.DataSource> GetDataSourceById(long id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var source = await TenantContext.GetTenantRepository(CurrentUser.Tenant).DataSources.GetAll().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
            return new DataSource
            {
                Id = source.Id,
                Type = source.ServerType,
                Name = source.Name,
                ColumnMapping = JsonConvert.DeserializeObject<List<ColumnMapping>>(source.ColumnMappings),
                ValidationSchema = source.ValidationSchema
            };
        }


        /// <summary>
        /// DataSource Measures
        /// </summary>
        /// <param name="id">DataSource Id</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpGet("{id}/Measures")]
        public async Task<IEnumerable<Measure>> GetMetrics(long id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var metrics = await TenantContext.GetTenantRepository(CurrentUser.Tenant).Measures.GetAll().Where(m => m.DataSourceId == id).ToListAsync(cancellationToken);
            return metrics.Select(m => new Measure
            {
                Id = m.Id,
                Name = m.Name,
                Query = m.Query,
            });
        }

        /// <summary>
        /// DataSource Dimensions
        /// </summary>
        /// <param name="id">DataSource Id</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpGet("{id}/Dimensions")]
        public async Task<IEnumerable<Dimension>> GetDimensions(long id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var dimensions = await TenantContext.GetTenantRepository(CurrentUser.Tenant).Dimensions.GetAll().Where(d => d.DataSourceId == id).ToListAsync(cancellationToken);
            return dimensions.Select(m => new Dimension
            {
                Id = m.Id,
                Name = m.Name
            });
        }

        /// <summary>
        /// Olap Measues
        /// </summary>
        /// <param name="dataSource"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("Measures")]
        [Versions("v2")]
        public async Task<IEnumerable<Measure>> GetDataSourceMeasures([FromBody]InputDataSource dataSource, CancellationToken cancellationToken = default(CancellationToken))
        {
            var olapAdapter = Helpers.Adapters.OlapAdapters[dataSource.Adapter];
            var olapConnection = new OlapConnection { Server = dataSource.Server, Database = dataSource.Database, Cube = dataSource.Cube };
            olapConnection = await OlapAdapterHelpers.TransformConnectionForAdapter(dataSource.Adapter, CurrentUser.Tenant, 0, olapConnection, Configuration, HttpContext, Request, cancellationToken);
            return (await olapAdapter.GetMeasuresAsync(olapConnection)).Select(m => new Measure
            {
                Name = m.Key,
                Query = m.Value
            });
        }

        /// <summary>
        /// Olap Dimensions
        /// </summary>
        /// <param name="dataSource"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("Dimensions")]
        [Versions("v2")]
        public async Task<IEnumerable<Dimension>> GetDataSourceDimensions([FromBody]InputDataSource dataSource, CancellationToken cancellationToken = default(CancellationToken))
        {
            var olapAdapter = Helpers.Adapters.OlapAdapters[dataSource.Adapter];
            var olapConnection = new OlapConnection { Server = dataSource.Server, Database = dataSource.Database, Cube = dataSource.Cube };
            olapConnection = await OlapAdapterHelpers.TransformConnectionForAdapter(dataSource.Adapter, CurrentUser.Tenant, 0, olapConnection, Configuration, HttpContext, Request, cancellationToken);
            return (await olapAdapter.GetDimensionsAsync(olapConnection)).Select(d => new Dimension
            {
                Name = d.Caption,
                Query = d.UniqueName,
                IsDate = d.Date
            });
        }

        /// <summary>
        /// Olap Measures of DataSource
        /// </summary>
        /// <param name="id">DataSource Id</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{id}/CubeMeasures")]
        [Versions("v2")]
        public async Task<IEnumerable<Measure>> GetDataSourceMeasures(long id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var dataSource = await TenantContext.GetTenantRepository(CurrentUser.Tenant).DataSources.GetAll().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
            var olapAdapter = Helpers.Adapters.OlapAdapters[dataSource.ServerType];
            return (await olapAdapter.GetMeasuresAsync(new OlapConnection { Server = dataSource.Server, Database = dataSource.AnalysisDataBase, Cube = dataSource.Cube })).Select(m => new Measure
            {
                Name = m.Key,
                Query = m.Value
            });
        }

        /// <summary>
        /// Olap Dimensions of DataSource
        /// </summary>
        /// <param name="id">DataSource Id</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{id}/CubeDimensions")]
        [Versions("v2")]
        public async Task<IEnumerable<Dimension>> GetDataSourceDimensions(long id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var dataSource = await TenantContext.GetTenantRepository(CurrentUser.Tenant).DataSources.GetAll().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
            var olapAdapter = Helpers.Adapters.OlapAdapters[dataSource.ServerType];
            return (await olapAdapter.GetDimensionsAsync(new OlapConnection { Server = dataSource.Server, Database = dataSource.AnalysisDataBase, Cube = dataSource.Cube })).Select(d => new Dimension
            {
                Name = d.Caption,
                Query = d.UniqueName,
                IsDate = d.Date
            });
        }

        /// <summary>
        /// Dimension Items DataSource
        /// </summary>
        /// <param name="dataSourceId">DataSource Id</param>
        /// <param name="id">Dimension Id</param>
        /// <param name="query">Dimension Query</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{dataSourceId}/Dimensions/{id}")]
        [Versions("v2")]
        public async Task<List<string>> GetDimensionItems(long dataSourceId, long id, string query, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var dataSource = await tenantContext.DataSources.GetAll().FirstOrDefaultAsync(i => i.Id == dataSourceId, cancellationToken);
            var dimension = await tenantContext.Dimensions.GetAll().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
            var olapAdapter = Helpers.Adapters.OlapAdapters[dataSource.ServerType];
            dynamic additionalInfo = null;
            var connection = new OlapConnection
            {
                Server = dataSource.Server,
                Database = dataSource.AnalysisDataBase,
                Cube = dataSource.Cube,
                AdditionalData = additionalInfo
            };

            List<string> data = await CacheProvider.GetDimesnionItemsAsync(olapAdapter, connection, dimension.Query, null, cancellationToken);
            var filtered = data.Where(d => d.ToLower().Contains(query.ToLower())).Take(20).ToList();
            var result = filtered.Select(f => f).ToList();
            result.Insert(0, "All");
            return result;
        }


        /// <summary>
        /// Delete DataSource
        /// </summary>
        /// <param name="id">DataSource Id</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Versions("v2")]
        public async Task<IActionResult> DeleteDataSource(long id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);

            //TODO: Add Reports to this list
            var dataSource = await tenantContext.DataSources.GetAll().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
            if (dataSource.ServerType == "mondrianService")
            {
                var connectionStringObject = JsonConvert.DeserializeObject<SQLConnection>(dataSource.ConnectionString);
                connectionStringObject = await OlapAdapterHelpers.TransformSQLConnection(connectionStringObject, CurrentUser.Tenant, Configuration);
                ISQLAdapter adapter = Helpers.Adapters.SQLAdapters[connectionStringObject.Type];

                await adapter.DeleteTable(connectionStringObject, dataSource.TableName);
            }

            var widgets = await tenantContext.Widgets.GetAll().ToListAsync(cancellationToken);
            var widgetsToDelete = widgets.Where(w => JsonConvert.DeserializeObject<List<long>>(w.DataSourceIds).Contains(id));
            var widgetsToDeleteIds = widgetsToDelete.Select(w => w.Id).ToList();
            var infoboardLinks = await tenantContext.InfoboardItems.GetAll().Where(il => widgetsToDeleteIds.Contains(il.WidgetId)).ToListAsync(cancellationToken);

            var measures = await tenantContext.Measures.GetAll().Where(m => m.DataSourceId == id).ToListAsync(cancellationToken);
            var dimensions = await tenantContext.Dimensions.GetAll().Where(d => d.DataSourceId == id).ToListAsync(cancellationToken);

            var dataReports = await tenantContext.DataReports.GetAll().Where(dr => dr.DataSourceId == id).ToListAsync(cancellationToken);
            var dataReportIds = dataReports.Select(d => d.Id).ToList();
            var reports = await 
                tenantContext.Reports.GetAll().Where(r => dataReportIds.Contains(r.ReportId)).ToListAsync(cancellationToken);

            var formulae = await tenantContext.Formula.GetAll().Where(f => f.DataSourceId == id).ToListAsync(cancellationToken);
            var formulaIds = formulae.Select(f => f.Id);
            var formulaElements = await tenantContext.FormulaElements.GetAll().Where(fe => formulaIds.Contains(fe.FormulaId)).ToListAsync(cancellationToken);
            var formulaElementDimensions = await tenantContext.FormulaElementDimensions.GetAll().Where(fe => formulaIds.Contains(fe.FormulaId)).ToListAsync(cancellationToken);
            var analysis = await tenantContext.Analysis.GetAll().Where(a => formulaIds.Contains(a.FormulaId)).ToListAsync(cancellationToken);
            var analysisIds = analysis.Select(a => a.Id);
            var analysisDimensions = await tenantContext.AnalysisDimensions.GetAll().Where(ad => analysisIds.Contains(ad.AnalysisId)).ToListAsync(cancellationToken);
            var analysisDimensionItems = await tenantContext.AnalysisDimensionItems.GetAll().Where(ad => analysisIds.Contains(ad.AnalysisId)).ToListAsync(cancellationToken);
            var analysisSaves = await tenantContext.AnalysisScenarios.GetAll().Where(ad => analysisIds.Contains(ad.AnalysisId)).ToListAsync(cancellationToken);

            //TODO: Should be Wrapped into Transaction and Rollback

            tenantContext.InfoboardItems.Delete(infoboardLinks);
            tenantContext.Widgets.Delete(widgetsToDelete);
            tenantContext.DataReports.Delete(dataReports);
            tenantContext.Reports.Delete(reports);
            tenantContext.AnalysisScenarios.Delete(analysisSaves);
            tenantContext.AnalysisDimensionItems.Delete(analysisDimensionItems);
            tenantContext.AnalysisDimensions.Delete(analysisDimensions);
            tenantContext.Analysis.Delete(analysis);
            tenantContext.FormulaElementDimensions.Delete(formulaElementDimensions);
            tenantContext.FormulaElements.Delete(formulaElements);
            tenantContext.Formula.Delete(formulae);
            tenantContext.Dimensions.Delete(dimensions);
            tenantContext.Measures.Delete(measures);
            tenantContext.DataSources.Delete(dataSource);
            await tenantContext.CommitAsync();

            string configurationFolder = Configuration.Application.Paths["Reports"];
            var file = System.IO.Path.Combine(configurationFolder, CurrentUser.Tenant, "ColorRegistry.json");
            if (System.IO.File.Exists(file))
            {
                var map = JsonConvert.DeserializeObject<List<ColorPaletteMap>>(System.IO.File.ReadAllText(file));
                map = map.Where(m => m.DataSourceId != id).ToList();
                System.IO.File.WriteAllText(file, JsonConvert.SerializeObject(map));
            }
            return Ok();
        }


        /// <summary>
        /// Get Calculated Measures
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{id}/CalculatedMeasures")]
        [Versions("v2")]
        public async Task<List<CalculatedMeasure>> GetCalculatedMeasures(long id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var measures = await tenantContext.Measures.GetAll().Where(m => m.DataSourceId == id && string.IsNullOrEmpty(m.Aggregation)).ToListAsync();
            return measures.Select(m => new CalculatedMeasure
            {
                Id = m.Id,
                Name = m.Name,
                Description = m.Description,
                Query = m.Query,
                Formula = m.DataQuery,
                Prefix = m.Prefix,
                Suffix = m.Suffix,
                IsPercent = bool.Parse(m.IsPercent),
            }).ToList();

        }

        /// <summary>
        /// Add New Calculated Measure
        /// </summary>
        /// <param name="id"></param>
        /// <param name="measure"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("{id}/CalculatedMeasures")]
        [Versions("v2")]
        public async Task<CalculatedMeasure> AddCalculatedMeasures(long id, [FromBody]CalculatedMeasure measure, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            Models.Measure saveMeasure = new Models.Measure()
            {
                Name = measure.Name,
                Description = measure.Description,
                Aggregation = null,
                DataQuery = measure.Formula,
                ColumnName = null,
                Prefix = measure.Prefix,
                Suffix = measure.Suffix,
                IsPercent = measure.IsPercent.ToString(),
                CreatedOn = DateTime.Now,
                DataSourceId = id,
                Query = measure.Query
            };
            tenantContext.Measures.Add(saveMeasure);
            await tenantContext.CommitAsync();
            await this.FlushCache(CurrentUser.Tenant, id, cancellationToken);
            var m = saveMeasure;
            return new CalculatedMeasure
            {
                Id = m.Id,
                Name = m.Name,
                Description = m.Description,
                Query = m.Query,
                Formula = m.DataQuery,
                Prefix = m.Prefix,
                Suffix = m.Suffix,
                IsPercent = bool.Parse(m.IsPercent),
            };

        }

        /// <summary>
        /// Update Calculated Measure
        /// </summary>
        /// <param name="id"></param>
        /// <param name="measureId"></param>
        /// <param name="measure"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut("{id}/CalculatedMeasures/{measureId}")]
        [Versions("v2")]
        public async Task<CalculatedMeasure> UpdateCalculatedMeasures(long id, long measureId, [FromBody]CalculatedMeasure measure, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var saveMeasure = await tenantContext.Measures.GetAll().FirstOrDefaultAsync(me => me.Id == measureId);
            if (saveMeasure == null) throw new Exception("DS-005", new Exception("Calculated Measure Not Found"));
            saveMeasure.Name = measure.Name;
            saveMeasure.Description = measure.Description;
            saveMeasure.Aggregation = null;
            saveMeasure.DataQuery = measure.Formula;
            saveMeasure.ColumnName = null;
            saveMeasure.Prefix = measure.Prefix;
            saveMeasure.Suffix = measure.Suffix;
            saveMeasure.IsPercent = measure.IsPercent.ToString();
            saveMeasure.Query = measure.Query;
            tenantContext.Measures.Update(saveMeasure);
            await tenantContext.CommitAsync();
            await this.FlushCache(CurrentUser.Tenant, id, cancellationToken);
            var m = saveMeasure;
            return new CalculatedMeasure
            {
                Id = m.Id,
                Name = m.Name,
                Description = m.Description,
                Query = m.Query,
                Formula = m.DataQuery,
                Prefix = m.Prefix,
                Suffix = m.Suffix,
                IsPercent = bool.Parse(m.IsPercent),
            };

        }

        /// <summary>
        /// Delete Calulated Measure
        /// </summary>
        /// <param name="id"></param>
        /// <param name="measureId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete("{id}/CalculatedMeasures/{measureId}")]
        [Versions("v2")]
        public async Task<IActionResult> DeleteCalculatedMeasures(long id, long measureId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var saveMeasure = await tenantContext.Measures.GetAll().FirstOrDefaultAsync(me => me.Id == measureId);
            if (saveMeasure == null) throw new Exception("DS-005", new Exception("Calculated Measure Not Found"));
            tenantContext.Measures.Delete(saveMeasure);
            await tenantContext.CommitAsync();
            await this.FlushCache(CurrentUser.Tenant, id, cancellationToken);
            return Ok();

        }

        /// <summary>
        /// Internal
        /// </summary>
        /// <param name="tenant"></param>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async Task FlushCache(string tenant, long id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(tenant);
            var dataSource = await tenantContext.DataSources.GetAll().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
            var olapAdapter = Helpers.Adapters.OlapAdapters[dataSource.ServerType];
            var olapConnection = new OlapConnection
            {
                Server = dataSource.Server,
                Database = dataSource.AnalysisDataBase,
                Cube = dataSource.Cube,
            };
            olapConnection = await OlapAdapterHelpers.TransformConnectionForAdapter(dataSource.ServerType, CurrentUser.Tenant, id, olapConnection, Configuration, HttpContext, Request, cancellationToken);
            try
            {
                await olapAdapter.FlushCache(olapConnection);
                await CacheProvider.RefreshDataSourceAsync(olapAdapter, olapConnection);
            }
            catch (Exception ex)
            {
                logger.LogError("Error While flushing cache", ex);
            }
        }



        /// <summary>
        /// Toggle DataSource Sharing with team.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut("{id}/Share")]
        [Versions("v2")]
        public async Task<IActionResult> ToggleDataSourceShare(long id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var dataSource = await tenantContext.DataSources.GetAll().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
            dataSource.IsPublic = !dataSource.IsPublic;
            tenantContext.DataSources.Update(dataSource);
            await tenantContext.CommitAsync();
            return Ok();

        }


#region "MondrianDirect Related"


        /// <summary>
        /// Validate Mondrian Schema
        /// </summary>
        /// <param name="file"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("MondrianDirect/ValidateSchema")]
        [Versions("v2")]
        public async Task<IActionResult> ValidateMondrianSchema(IFormFile file, CancellationToken cancellationToken = default(CancellationToken))
        {
            string ServerUploadFolder = Configuration.Application.Paths["Reports"];
            var newFilename = "Cube" + Guid.NewGuid() + ".xml";
            var fileName = Path.Combine(ServerUploadFolder, CurrentUser.Tenant, newFilename);
            await file.SaveAsAsync(fileName);
#if NET461
            System.Xml.Schema.XmlSchemaSet schemaSet = new System.Xml.Schema.XmlSchemaSet();
            schemaSet.Add("", Path.Combine(ServerUploadFolder, "mondrian4.xsd"));
            System.Xml.Schema.XmlSchema compiledSchema = null;
            List<string> errorMessages = new List<string>();

            foreach (System.Xml.Schema.XmlSchema schema in schemaSet.Schemas())
            {
                compiledSchema = schema;
            }
            try
            {
                System.Xml.XmlReaderSettings settings = new System.Xml.XmlReaderSettings();
                settings.Schemas.Add(compiledSchema);
                settings.ValidationEventHandler += (object sender, System.Xml.Schema.ValidationEventArgs args) =>
                {
                    // errorMessages.Add(args.Message);
                };
                settings.ValidationType = System.Xml.ValidationType.Schema;

                //Create the schema validating reader.
                System.Xml.XmlReader vreader = System.Xml.XmlReader.Create(fileName, settings);

                while (vreader.Read()) { }

                //Close the reader.
                vreader.Close();

                XDocument doc = XDocument.Load(fileName);
                var schemaName = doc.Descendants("Schema").FirstOrDefault().Attributes("name").FirstOrDefault().Value;
                var cubeNames = doc.Descendants("Cube").Where(c => c.Attributes("visible").FirstOrDefault() == null || c.Attributes("visible").FirstOrDefault().Value != "false").Select(c => c.Attributes("name").FirstOrDefault().Value);
                var virtualCubes = doc.Descendants("VirtualCube").Where(c => c.Attributes("visible").FirstOrDefault() == null || c.Attributes("visible").FirstOrDefault().Value != "false").Select(c => c.Attributes("name").FirstOrDefault().Value);
                if (errorMessages.Count == 0)
                {
                    return Ok(new SchemaResult
                    {
                        FileName = newFilename,
                        AnalysisDatabase = schemaName,
                        CubeNames = cubeNames.Union(virtualCubes).ToArray()
                    });
                }
                return BadRequest(errorMessages);
            }
            catch (Exception ex)
            {

                errorMessages.Add(ex.Message);
                return BadRequest(errorMessages);
            }
#endif
#if !NET461
            XDocument doc = XDocument.Load(fileName);
            var schemaName = doc.Descendants("Schema").FirstOrDefault().Attributes("name").FirstOrDefault().Value;
            var cubeNames = doc.Descendants("Cube").Select(c => c.Attributes("name").FirstOrDefault().Value);
            return Ok(new SchemaResult
            {
                FileName = newFilename,
                AnalysisDatabase = schemaName,
                CubeNames = cubeNames.ToArray()
            });
#endif

        }



        /// <summary>
        /// Get MondrianDirect Measures and Dimensions
        /// </summary>
        /// <param name="inputDataSource"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("MondrianDirect/MeasureDimensions")]
        [Versions("v2")]
        public async Task<MondrianDirectMeasuresAndDimensions> GetMondiranDirectMeasureDimensions([FromBody]MondrianDirectDataSource inputDataSource, CancellationToken cancellationToken = default(CancellationToken))
        {


            var olapConnection = new OlapConnection
            {
                Server = inputDataSource.Server,
                Database = inputDataSource.Database,
                Cube = inputDataSource.Cube,
                AdditionalData = inputDataSource.FileName,
            };
            olapConnection = await OlapAdapterHelpers.TransformConnectionForAdapter(inputDataSource.Adapter, CurrentUser.Tenant, 0, olapConnection, Configuration, HttpContext, Request, cancellationToken);

            var adapter = Helpers.Adapters.OlapAdapters["mondrianService"];
            var measures = await adapter.GetMeasuresAsync(olapConnection);
            var dimensions = await adapter.GetDimensionsAsync(olapConnection);

            var dimensionsGrouped = dimensions.GroupBy(d => d.DimensionName);

            var dimensionResult = new List<Dimension>();
            var dimensionsAdded = new List<string>();
            foreach (var dimension in dimensions)
            {
                if (dimension.Date)
                {
                    if (!dimensionsAdded.Contains(dimension.DimensionName))
                    {
                        dimensionsAdded.Add(dimension.DimensionName);
                        dimensionResult.Add(new Dimension { Name = dimension.DimensionName.Replace("[", "").Replace("]", ""), IsDate = true, Query = dimension.DimensionName });
                    }
                }
                else
                {
                    dimensionResult.Add(new Dimension { Name = dimension.Caption, IsDate = false, Query = dimension.UniqueName });
                }
            }
            return new MondrianDirectMeasuresAndDimensions
            {
                Measures = measures.Select(m => new Measure { Name = m.Key, Query = m.Value }).ToList(),
                Dimensions = dimensionResult
            };
        }

        /// <summary>
        /// Create a New Mondiran Direct Source
        /// </summary>
        /// <param name="dataSource"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("MondrianDirect")]
        [Versions("v2")]
        public async Task<DataSource> AddMondrianDirectSource([FromBody]MondrianDirectDataSourceWithMesuresAndDimensions dataSource, CancellationToken cancellationToken = default(CancellationToken))
        {

            string adpater = Configuration.Application.AnalyticsData.SQLAdapter;
            string name = dataSource.Name;
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var existing = await tenantContext.DataSources.GetAll().Where(d => d.Name.ToLower() == name.ToLower()).AnyAsync(cancellationToken);
            if (existing)
            {
                throw new Exception("DS-001", new Exception("Data Source with same name already exists"));
            }
            List<string> timeDimensions = new List<string>();
            var dataSourceToSave = new Models.DataSource()
            {
                CreatedOn = DateTime.UtcNow,
                Name = dataSource.Name,
                TypeId = 80,
                ServerType = "mondrianDirect",
                ConnectionString = dataSource.FileName,
                Server = dataSource.Server,
                AnalysisDataBase = dataSource.Database,
                Cube = dataSource.Cube,
                CreatedBy = CurrentUser.Id,
                ColumnMappings = null,
                ValidationSchema = null,
                TableName = name,
                Measures = dataSource.Measures.Select(measure => new Infoveave.Models.Measure
                {
                    CreatedOn = DateTime.Now,
                    Suffix = !measure.IsPrefix ? measure.Uom : null,
                    Prefix = measure.IsPrefix ? measure.Uom : null,
                    Name = measure.Name,
                    ColumnName = null,
                    Description = measure.Name,
                    Aggregation = null,
                    Query = measure.Query,
                    DataQuery = null,
                }).ToList(),
                Dimensions = dataSource.Dimensions.Select(dim => new Infoveave.Models.Dimension
                {
                    Name = dim.Name,
                    Query = dim.Query,
                    DrillDownLevel = 0,
                    IsDate = dim.IsDate,
                    IsRangeDimension = false,
                    ColumnName = null,
                    RangeDimension = "",
                }).ToList()
            };
            tenantContext.DataSources.Add(dataSourceToSave);
            await tenantContext.CommitAsync();
            return new DataSource()
            {
                Id = dataSourceToSave.Id,
                Name = dataSourceToSave.Name,
                Type = dataSourceToSave.ServerType
            };
        }

        #endregion

    }
}
