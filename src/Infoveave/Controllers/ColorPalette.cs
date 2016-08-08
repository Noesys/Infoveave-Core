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
using Infoveave.Helpers;
using Infoveave.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infoveave.Controllers
{
    /// <summary>
    /// Color Palette Management
    /// </summary>
    [Authorize]
    [Route("api/{version}/ColorPalette")]
    public class ColorPaletteController : BaseController
    {
        private readonly IHostingEnvironment _appEnvironment;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        /// <param name="appEnvironment"></param>
        public ColorPaletteController(ITenantContext context, IOptions<ApplicationConfiguration> configuration, IHostingEnvironment appEnvironment)
            : base(context, configuration: configuration)
        {
            _appEnvironment = appEnvironment;
        }


        /// <summary>
        /// Get Color Palette Map
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("")]
        [Versions("v2")]
        public List<ColorPaletteMap> GetColorPalette(CancellationToken cancellationToken = default(CancellationToken))
        {
            string configurationFolder = Configuration.Application.Paths["Reports"];
            var file = System.IO.Path.Combine(configurationFolder, CurrentUser.Tenant, "ColorRegistry.json");
            if (System.IO.File.Exists(file))
            {
                return JsonConvert.DeserializeObject<List<ColorPaletteMap>>(System.IO.File.ReadAllText(file));
            }
            return new List<ColorPaletteMap>();
        }

        /// <summary>
        /// Get Color Palette for Tenant
        /// </summary>
        /// <param name="tenant"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("{tenant}")]
        [Versions("v2")]
        public List<ColorPaletteMap> GetColorPaletteForTenant(string tenant, CancellationToken cancellationToken = default(CancellationToken))
        {
            string configurationFolder = Configuration.Application.Paths["Reports"];
            var file = System.IO.Path.Combine(configurationFolder, tenant, "ColorRegistry.json");
            if (System.IO.File.Exists(file))
            {
                return JsonConvert.DeserializeObject<List<ColorPaletteMap>>(System.IO.File.ReadAllText(file));
            }
            return new List<ColorPaletteMap>();
        }

        /// <summary>
        /// Save Color Palette Map
        /// </summary>
        /// <param name="palette"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("")]
        [Versions("v2")]
        public IActionResult SaveColorPalette([FromBody]List<ColorPaletteMap> palette, CancellationToken cancellationToken = default(CancellationToken))
        {
            string configurationFolder = Configuration.Application.Paths["Reports"];
            var file = System.IO.Path.Combine(configurationFolder, CurrentUser.Tenant, "ColorRegistry.json");
            System.IO.File.WriteAllText(file, JsonConvert.SerializeObject(palette));
            return Ok();
        }
    }
}
