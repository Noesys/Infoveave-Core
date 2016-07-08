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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Infoveave.Controllers
{
    /// <summary>
    /// Infoboard Controller
    /// </summary>
    [Authorize]
    [Route("api/{version}/Infoboards")]
    public class InfoboardController : BaseController
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        /// <param name="cacheProvider"></param>
        public InfoboardController(ITenantContext context, IOptions<ApplicationConfiguration> configuration, CacheProvider.ICacheProvider cacheProvider)
            : base(context, configuration:configuration,cacheProvider:cacheProvider)
        {
        }

        /// <summary>
        /// Get Infoboards
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("")]
        [Versions("v2")]
        public async Task<IEnumerable<Infoboard>> GetInfoboards(CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var infoboards = await tenantContext.Infoboards.GetAll().OrderBy(i=>i.SortOrder).Where(i => i.CreatedBy == CurrentUser.Id || i.IsPublic == true).ToListAsync(cancellationToken);
            return infoboards.Select(i => new Infoboard()
            {
                Id = i.Id,
                Name = i.Name,
                ShortCode = i.ShortCode,
                SortOrder = i.SortOrder
             });
        }


        /// <summary>
        /// Get Infoboard by Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Versions("v2")]
        public async Task<InfoboardDisplay> GetInfoboard(long id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext =  TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var infoboard = await tenantContext.Infoboards.GetAll().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
            if (infoboard == null) return null;
            var infoboardItems = await tenantContext.InfoboardItems.GetAll().Where(i => i.InfoboardId == id).ToListAsync(cancellationToken);
            var dataSources = await tenantContext.DataSources.GetAll().ToListAsync(cancellationToken);
            var dataSourceIds = dataSources.Select(ds => ds.Id).ToList();
            var measures = await tenantContext.Measures.GetAll().Where(m => dataSourceIds.Contains(m.DataSourceId)).ToListAsync(cancellationToken);
            var dimensions = await tenantContext.Dimensions.GetAll().Where(di => dataSourceIds.Contains(di.DataSourceId)).ToListAsync(cancellationToken);
            return new InfoboardDisplay
            {
                Id = infoboard.Id,
                Name = infoboard.Name,
                ShortCode = infoboard.ShortCode,
                Options = infoboard.InfoboardOptions,
                Layouts = infoboard.Layouts,
                Items = infoboardItems.Select(i => new InfoboardItem
                {
                    Id = i.Id,
                    W = i.HorizontalSize,
                    H = i.VerticalSize,
                    X = i.PositionX,
                    Y = i.PositionY,
                    WidgetId = i.WidgetId,
                }),
                Measures = measures.Select(m => new InfoboardMeasure
                {
                    Id = m.Id,
                    Name = m.Name,
                    Query = m.Query,
                    DataSourceId = m.DataSourceId,
                }),
                Dimensions = dimensions.Select(d => new InfoboardDimension
                {
                    Id = d.Id,
                    Name = d.Name,
                    Query = d.Query,
                    IsDate = d.IsDate,
                    DataSourceId = d.DataSourceId,
                }).Union(new InfoboardDimension[1] { new InfoboardDimension { Id = 0, IsDate = true, Name = "Date", Query = "Date" } })
            };
        }


        /// <summary>
        /// Add Infoboard
        /// </summary>
        /// <param name="board"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpPost("")]
        public async Task<Infoboard> AddInfoboard([FromBody]Infoboard board, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var infoboard = new Models.Infoboard();
            var CountInfo = tenantContext.Infoboards.GetAll().Count();
            if (CountInfo == 0)
            {
                infoboard.SortOrder = 0;
            }
            else
            {
                var maxsort = tenantContext.Infoboards.GetAll().Max(x => x.SortOrder);
                if (maxsort == null) maxsort = 0;
                infoboard.SortOrder = maxsort + 1;
            }
            infoboard.Name = board.Name;
            infoboard.CreatedBy = CurrentUser.Id;
            infoboard.CreatedOn = DateTime.Now;
            infoboard.InfoboardOptions = "{\"schema\":2,\"dateRange\":{\"mode\":999,\"beginDate\":null,\"endDate\":null,\"progression\":201},\"filters\":[]}";
            infoboard.ShortCode = ShortCodeHelper.GetShortCode(6);
            
            tenantContext.Infoboards.Add(infoboard);

            await tenantContext.CommitAsync();
            return new Infoboard
            {
                Id = infoboard.Id,
                Name = infoboard.Name,
                Options = infoboard.InfoboardOptions,
                ShortCode = infoboard.ShortCode
            };
        }

        /// <summary>
        /// Sort Infoboards
        /// </summary>
        /// <param name="boards"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpPut("Sort")]
        public async Task<IActionResult> SortInfoboards([FromBody]List<Infoboard> boards, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var infoboards = await tenantContext.Infoboards.GetAll().ToListAsync(cancellationToken);
            foreach (var infoboard in infoboards)
            {
                var filter = boards.FirstOrDefault(x => x.Id == infoboard.Id);
                if (filter == null) continue;
                infoboard.SortOrder = filter.SortOrder;
                tenantContext.Infoboards.Update(infoboard);
              
            };
            await tenantContext.CommitAsync();
            return Ok();
        }

        /// <summary>
        /// Save Infoboard Widget Positions
        /// </summary>
        /// <param name="id"></param>
        /// <param name="items"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpPost("{id}/UpdateState")]
        public async Task<IActionResult> SaveWidgetState(long id, [FromBody]InfoboardItem[] items, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var itemIds = items.Select(i => i.Id).ToList();
            var infoboardItems = tenantContext.InfoboardItems.GetAll().Where(i => itemIds.Contains(i.Id));
            foreach (var item in infoboardItems)
            {
                var newValue = items.First(i => i.Id == item.Id);
                item.PositionX = newValue.X;
                item.PositionY = newValue.Y;
                item.HorizontalSize = newValue.W;
                item.VerticalSize = newValue.H;
            };
            await tenantContext.CommitAsync();
            return Ok();
        }

        /// <summary>
        /// Update Layout
        /// </summary>
        /// <param name="id"></param>
        /// <param name="board"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpPut("{id}/UpdateLayout")]
        public async Task<IActionResult> UpdateLayout(long id, [FromBody]Infoboard board, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var infoboard = await tenantContext.Infoboards.GetAll().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
            infoboard.Layouts = board.Layouts;
            await tenantContext.CommitAsync();
            return Ok();
        }

        /// <summary>
        /// Update Infoboard
        /// </summary>
        /// <param name="id"></param>
        /// <param name="board"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpPut("{id}")]
        public async Task<Infoboard> UpdateInfoboard(long id, [FromBody]Infoboard board, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var infoboard = await tenantContext.Infoboards.GetAll().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
            infoboard.Name = (!string.IsNullOrEmpty(board.Name)) ? board.Name : infoboard.Name;
            infoboard.InfoboardOptions = (!string.IsNullOrEmpty(board.Options)) ? board.Options : infoboard.InfoboardOptions;
            await tenantContext.CommitAsync();
            return new Infoboard
            {
                Id = infoboard.Id,
                Name = infoboard.Name,
                Options = infoboard.InfoboardOptions,
                ShortCode = infoboard.ShortCode
            };
        }
        /// <summary>
        /// DeleteInfoBoard
        /// </summary>
        /// <param name="id"></param>
     
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInfoBoard(long id,  CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var infoboard = await tenantContext.Infoboards.GetAll().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
            if (infoboard == null)
            {
                throw new Exception("RES-I003");
            }
            var infoboardItems = tenantContext.InfoboardItems.GetAll().Where(bi => bi.InfoboardId == infoboard.Id).ToList();
            foreach (var link in infoboardItems)
            {
                tenantContext.InfoboardItems.Delete(link);              
            }
            tenantContext.Infoboards.Delete(infoboard);
            await tenantContext.CommitAsync(cancellationToken);
            return Ok();
        }
        /// <summary>
        /// Add Widget to Infoboard
        /// </summary>
        /// <param name="id"></param>
        /// <param name="item"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpPost("{id}/Items")]
        public async Task<InfoboardItem> AddWidgetToBoard(long id, [FromBody]InfoboardItem item, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var itemToAdd = new Infoveave.Models.InfoboardLink
            {
                InfoboardId = id,
                WidgetId = item.WidgetId,
                PositionX = item.X,
                PositionY = item.Y,
                HorizontalSize = item.W,
                VerticalSize = item.H
            };
            tenantContext.InfoboardItems.Add(itemToAdd);
            await tenantContext.CommitAsync(cancellationToken);
            item.Id = itemToAdd.Id;
            return item;
        }

        /// <summary>
        /// Delete Widget from Board
        /// </summary>
        /// <param name="id"></param>
        /// <param name="linkId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpDelete("{id}/Items/{linkId}")]
        public async Task<IActionResult> RemoveWidgetFromBoard(long id, long linkId,CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var link = tenantContext.InfoboardItems.GetAll().FirstOrDefault(i => i.Id == linkId);
            tenantContext.InfoboardItems.Delete(link);
            await tenantContext.CommitAsync(cancellationToken);
            return Ok();
        }

        
        /// <summary>
        /// Get All available Widets
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpGet("Widgets")]
        public async Task<IEnumerable<Widget>> GetWidgets(CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var widgets = await tenantContext.Widgets.GetAll().Where(w => w.CreatedBy == CurrentUser.Id || w.IsPublic == true).ToListAsync(cancellationToken);
            return widgets.Select(w => new Widget
            {
                Id = w.Id,
                Type = w.ItemType,
                Description = w.FullName,
                Name = w.Name,
                Data = w.SavedData,
                DataSourceIds = w.DataSourceIds,
                ShortCode = w.ShortCode,
                IsPublic = w.IsPublic
            });
        }


        /// <summary>
        /// Create New Widget
        /// </summary>
        /// <param name="widget"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpPost("Widgets")]
        public async Task<Widget> CreateWidget([FromBody]Widget widget, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var widgetToAdd = new Models.Widget()
            {
                CreatedBy = CurrentUser.Id,
                CreatedOn = DateTime.Now,
                IsPublic = false,
                Name = widget.Name,
                ItemType = widget.Type,
                SavedData = widget.Data,
                DataSourceIds = widget.DataSourceIds,
                ShortCode = ShortCodeHelper.GetShortCode(8),
                FullName = widget.Description,
            };
            tenantContext.Widgets.Add(widgetToAdd);
            await tenantContext.CommitAsync(cancellationToken);
            widget.Id = widgetToAdd.Id;
            widget.ShortCode = widgetToAdd.ShortCode;
            return widget;
        }


        /// <summary>
        /// Get Available DataSources
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpGet("DataSources")]
        public async Task<IEnumerable<DataSource>> GetDataSources(CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var dataSources = await tenantContext.DataSources.GetAll().Where(d => d.CreatedBy == CurrentUser.Id || d.IsPublic == true).ToListAsync(cancellationToken);
            var dataSourceIds = dataSources.Select(d => d.Id).ToList();
            var measures = await tenantContext.Measures.GetAll().Where(m => dataSourceIds.Contains(m.DataSourceId)).ToListAsync(cancellationToken);            
            var dimensions = await tenantContext.Dimensions.GetAll().Where(d => dataSourceIds.Contains(d.DataSourceId)).ToListAsync(cancellationToken);
            return dataSources.Select(ds => new DataSource
            {
                Id = ds.Id,
                Name = ds.Name,
                Type = ds.ServerType,
                Measures = measures.Where(m=> m.DataSourceId == ds.Id).Select(m => new Measure
                {
                    Id = m.Id,
                    Name = m.Name,
                    Query = m.Query,
                    DataSourceId = m.DataSourceId
                }),
                Dimensions = dimensions.Where(d => d.DataSourceId == ds.Id).Select(d => new Dimension
                {
                    Id = d.Id,
                    Name = d.Name,
                    Query = d.Query,
                    IsDate = d.IsDate,
                    DataSourceId = d.DataSourceId
                })
            });
        }

        /// <summary>
        /// Dimension Items of a DataSource
        /// </summary>
        /// <param name="dataSourceId"></param>
        /// <param name="id"></param>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")]
        [HttpGet("DataSources/{dataSourceId}/Dimensions/{id}")]
        public async Task<List<string>> GetDimensionItems(long dataSourceId, long id, string query, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var dataSource = await tenantContext.DataSources.GetAll().FirstOrDefaultAsync(i => i.Id == dataSourceId, cancellationToken);
            var dimension = await tenantContext.Dimensions.GetAll().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
            var userContext = (CurrentUser != null) ? (await tenantContext.Users.GetAll().FirstOrDefaultAsync(i => i.Id == CurrentUser.Id, cancellationToken)).Context : new List<Models.UserContext>();

            var filtered = new List<string>();
            // See if the dimension being requested is set as context if so just use to return that

            var context = (userContext == null) ? null : userContext.FirstOrDefault(uc => uc.DataSourceId == dataSourceId && uc.DimensionId == id);
            if (context != null)
            {
                filtered = context.Items.ToList();
            }
            else
            {
                var adapterToUse = (dataSource.ServerType == "mondrianDirect") ? "mondrianService" : dataSource.ServerType;
                var olapAdapter = Helpers.Adapters.OlapAdapters[adapterToUse];
                var connection = new AdapterFramework.OlapConnection
                {
                    Server = dataSource.Server,
                    Database = dataSource.AnalysisDataBase,
                    Cube = dataSource.Cube,
                    AdditionalData = (dataSource.ServerType == "mondrianDirect") ? dataSource.ConnectionString : null,
                };
                connection = await OlapAdapterHelpers.TransformConnectionForAdapter(dataSource.ServerType, CurrentUser.Tenant, dataSource.Id, connection, Configuration, HttpContext, Request, cancellationToken);
                List<string> data = await CacheProvider.GetDimesnionItemsAsync(olapAdapter, connection, dimension.Query, query, cancellationToken);
                filtered = data.ToList();
            }           
            if (!string.IsNullOrEmpty(query))
            {
                filtered = filtered.Where(d => d.ToLower().Contains(query.ToLower())).Take(100).ToList();
            }
            var result = filtered.Select(f => f).ToList();
            result.Insert(0, "All");
            return result;
        }



        /// <summary>
        /// Get All Users
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Versions("v2")][HttpGet("Users")]
        public async Task<List<User>> GetUsers(CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var users = await tenantContext.Users.GetAll().ToListAsync(cancellationToken);
            return users.Select(u => new User
            {
                Id = u.Id,
                UserName = u.FirstName + " " + u.LastName,
                ImagePath = u.ImagePath
            }).ToList();
        }


        /// <summary>
        /// Export Infoboards
        /// </summary>
        /// <remarks>
        /// Export Infoboards of the Current User
        /// </remarks>
        /// <returns></returns>
        [Versions("v2")][HttpPost("ExportDefault")]
        public async Task<IActionResult> ExportDefaultInfoboards()
        {
            var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
            var exportableWidgets = await tenantContext.Widgets.GetAll().Where(w => w.IsPublic == true && w.CreatedBy == CurrentUser.Id).ToListAsync();

            var infoboards = await tenantContext.Infoboards.GetAll()
                                .Include(i => i.Items)
                                .Where(i => i.CreatedBy == CurrentUser.Id).ToListAsync();
            if (infoboards.Count == 0 || exportableWidgets.Count == 0) { return BadRequest("Nothing to Export"); }
            infoboards = infoboards.Select(i =>
            {
                var ir = i;
                ir.Items = ir.Items.Where(il => exportableWidgets.Select(ew => ew.Id).Contains(il.WidgetId)).ToList();
                return ir;
            }).ToList();
            var content = JsonConvert.SerializeObject(infoboards, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            System.IO.File.WriteAllText(System.IO.Path.Combine(Configuration.Application.Paths["Reports"],CurrentUser.Tenant,"defaultInfoboards.json"),content);
            return Ok(exportableWidgets.Count);
        }

        /// <summary>
        /// Import Infoboards
        /// </summary>
        /// <remarks>
        /// Import Infoboards which have been globally exported.
        /// </remarks>
        /// <returns></returns>
        [Versions("v2")][HttpPost("ImportDefault")]
        public async Task<IActionResult> ImportDefaultInfoboards()
        {
            var file = System.IO.Path.Combine(Configuration.Application.Paths["Reports"], CurrentUser.Tenant, "defaultInfoboards.json");
            if (!System.IO.File.Exists(file))
            {
                return BadRequest();
            }
            var content = System.IO.File.ReadAllText(file);
            try
            {
                var infoboards = JsonConvert.DeserializeObject<List<Models.Infoboard>>(content);
                var tenantContext = TenantContext.GetTenantRepository(CurrentUser.Tenant);
                // If here we know we can proceed
                var existingInfoboards = await tenantContext.Infoboards.GetAll().Where(i => i.CreatedBy == CurrentUser.Id).ToListAsync();
                existingInfoboards.ForEach(ei => tenantContext.Infoboards.Delete(ei));
                await tenantContext.CommitAsync();

                var newInfoboards = infoboards.Select(i =>
                {
                    return new Models.Infoboard()
                    {
                        Layouts = i.Layouts,
                        Name = i.Name,
                        InfoboardOptions = i.InfoboardOptions,
                        CreatedBy = CurrentUser.Id,
                        CreatedOn = DateTime.Now,
                        IsPublic = false,
                        ShortCode = null,
                        SortOrder = i.SortOrder,
                        Items = i.Items.Select(il =>
                        {
                            return new Models.InfoboardLink
                            {
                                HorizontalSize = il.HorizontalSize,
                                VerticalSize = il.VerticalSize,
                                WidgetId = il.WidgetId,
                                PositionX = il.PositionX,
                                PositionY = il.PositionY,
                                SavedViewShortCode = il.SavedViewShortCode,
                            };
                        }).ToList()
                    };
                }).ToList();
                newInfoboards.ForEach(ii =>
                {
                    tenantContext.Infoboards.Add(ii);
                });
                await tenantContext.CommitAsync();
                return Ok();
            }catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

}
