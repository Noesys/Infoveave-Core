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
using Infoveave.Data.Interfaces;
using System.Threading;
using System.Text;
using System.IO.Compression;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Infoveave.Controllers
{
    /// <summary>
    /// Services
    /// </summary>
    [Route("Services")]
    public class ServicesController : BaseController
    {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        public ServicesController(ITenantContext context) : base(context)
        {

        }

        /// <summary>
        /// Mondrian 4 Schema
        /// </summary>
        /// <param name="tenant"></param>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{tenant}/{id}/Mondrian4")]
        public async Task<IActionResult> Mondrian4(string tenant, long id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tenantContext = TenantContext.GetTenantRepository(tenant);
            var dataSource = await tenantContext.DataSources.GetAll().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
            var measures = await tenantContext.Measures.GetAll().Where(m => m.DataSourceId == dataSource.Id).ToListAsync(cancellationToken);
            var dimensions = await tenantContext.Dimensions.GetAll().Where(m => m.DataSourceId == dataSource.Id).ToListAsync(cancellationToken);
            var schemaTemplate = @"
<Schema metamodelVersion=""4.0"" name=""dbo"">
  <PhysicalSchema>
    <Table name=""{TableName}"">
      <ColumnDefs>
        <!--{CalculatedColumnDefs}-->
      </ColumnDefs>
    </Table>
     <Table name=""dimdate"">
      <Key>
        <Column name=""Date""/>
      </Key>
    </Table>
  </PhysicalSchema>
   <Dimension name=""InfoveaveDate"" table=""dimdate"" key=""Day"" type=""TIME"">
        <Attributes>
            <Attribute name=""Year"" levelType=""TimeYears"" keyColumn=""Year"" nameColumn=""YearName"" captionColumn=""YearName""/>
            <Attribute name=""Semester"" levelType=""TimeHalfYear"">
              <Key>
                    <Column name=""Year""/>
                    <Column name=""SemesterNo""/>
                </Key>
                <Name>
                    <Column name=""SemesterName""/>
                </Name>
                 <OrderBy>
                    <Column name=""Semester""/>
                </OrderBy>
            </Attribute>
            <Attribute name=""Quarter"" levelType=""TimeQuarters"">
               <Key>
                    <Column name=""Year""/>
                    <Column name=""QuarterNo""/>
                </Key>
                <Name>
                    <Column name=""QuarterName""/>
                </Name>
                 <OrderBy>
                    <Column name=""Quarter""/>
                </OrderBy>
            </Attribute>
            <Attribute name=""Month"" levelType=""TimeMonths"">
                  <Key>
                    <Column name=""Year""/>
                    <Column name=""MonthNo""/>
                </Key>
                <Name>
                    <Column name=""MonthName""/>
                </Name>
                 <OrderBy>
                    <Column name=""Month""/>
                </OrderBy>
            </Attribute>
            <Attribute name=""Week"" levelType=""TimeWeeks"">
                <Key>
                    <Column name=""Year""/>
                    <Column name=""WeekNo""/>
                </Key>
                <Name>
                    <Column name=""WeekName""/>
                </Name>
                 <OrderBy>
                    <Column name=""Week""/>
                </OrderBy>
            </Attribute>
            <Attribute name=""Day"" levelType=""TimeDays"" keyColumn=""Date"" nameColumn=""FormattedDate"" captionColumn=""FormattedDate""/>
        </Attributes>
    </Dimension>
  <Cube name=""{CubeName}"">
    <Dimensions>
       <!--{TimeDimensions}-->
       <!--{Dimensions}-->
    </Dimensions>
    <MeasureGroups>
      <MeasureGroup table=""{TableName}"">
        <Measures>
            <!--{Measures}-->
        </Measures>
         <DimensionLinks>
          <!--{DimensionLinks}-->
        </DimensionLinks>
      </MeasureGroup>
    </MeasureGroups>
    <CalculatedMembers>
         <!--{CalculatedMeasures}-->
    </CalculatedMembers>
  </Cube>
</Schema>";

            /*var calculatedColumnDefTemplate = @"<CalculatedColumnDef name=""{Column}"">
  <ExpressionView>
    <SQL dialect=""mssql"">
      Cast(FORMAT([{Column}],'yyyyMMdd') as varchar(10))
    </SQL>
    <SQL dialect=""mysql"">
      Cast(DATE_FORMAT({Column},'%Y%m%d') as char(10))
    </SQL>
  </ExpressionView>
</CalculatedColumnDef>";*/
            var measureTemplate = @"<Measure name=""{Name}"" column=""{Column}"" aggregator=""{Aggregator}"" visible=""true""/>";
            var calculatedMeasureTemplate = @"<CalculatedMember name=""{Name}"" dimension=""Measures""><Formula>{Formula}</Formula></CalculatedMember>";
            var dimensionTemplate = @"<Dimension name=""{Name}"" caption=""{Name}"" table=""{TableName}"" description="""" type=""OTHER"">
  <Attributes>
    <Attribute name=""{Name}"" caption=""{Name}"" description="""" keyColumn=""{Column}"" />
  </Attributes>
</Dimension>";

            var timeDimensionTemplate = @"<Dimension name=""{Name}"" source=""InfoveaveDate""/>";

            var timeDimensionLinkTemplate = @"<ForeignKeyLink dimension=""{Name}"" foreignKeyColumn=""{Column}_DateDim""/>";
            var dimensionLinkTemplate = @"<FactLink dimension=""{Name}""/>";
            //Creating ColumnDefs on Condition
            var calculatedColumnDefs = new List<string>();


            var measures4 = new List<string>();
            var calculatedMeasures4 = new List<string>();
            var dimensions4 = new List<string>();
            var timeDimensions4 = new List<string>();
            var dimensionLinks4 = new List<string>();
            var timeDimensionLinks4 = new List<string>();
            foreach (var measure in measures)
            {
                if (string.IsNullOrEmpty(measure.Aggregation))
                {
                    var calculatedMeasureb = new StringBuilder(calculatedMeasureTemplate);
                    calculatedMeasureb.Replace("{Name}", measure.Name);
                    calculatedMeasureb.Replace("{Formula}", measure.DataQuery.Replace("@",""));
                    calculatedMeasures4.Add(calculatedMeasureb.ToString());
                }
                else
                {
                    var measuresb = new StringBuilder(measureTemplate);
                    measuresb.Replace("{Name}", measure.Name);
                    measuresb.Replace("{Column}", measure.ColumnName);
                    measuresb.Replace("{Aggregator}", GetAggregation(measure.Aggregation));
                    measures4.Add(measuresb.ToString());
                }
                
            }
            foreach (var dimension in dimensions)
            {
                var dimensionsb = new StringBuilder(dimensionTemplate);
                var dimensionLinksb = new StringBuilder(dimensionLinkTemplate);
                if (dimension.IsDate)
                {
                    dimensionsb = new StringBuilder(timeDimensionTemplate);
                    dimensionLinksb = new StringBuilder(timeDimensionLinkTemplate);
                }
                dimensionsb.Replace("{TableName}", dataSource.TableName);
                dimensionsb.Replace("{Name}", dimension.Name);
                dimensionsb.Replace("{Column}", dimension.ColumnName);
                dimensionLinksb.Replace("{Name}", dimension.Name);
                dimensionLinksb.Replace("{Column}", dimension.ColumnName);
                dimensions4.Add(dimensionsb.ToString());
                dimensionLinks4.Add(dimensionLinksb.ToString());
            }
            var schema = new StringBuilder(schemaTemplate);
            schema.Replace("{TableName}", dataSource.TableName);
            schema.Replace("{CubeName}", dataSource.Cube);
            schema.Replace("<!--{CalculatedColumnDefs}-->", String.Join(Environment.NewLine, calculatedColumnDefs));
            schema.Replace("<!--{Measures}-->", String.Join(Environment.NewLine, measures4));
            schema.Replace("<!--{Dimensions}-->", String.Join(Environment.NewLine, dimensions4));
            schema.Replace("<!--{TimeDimensions}-->", String.Join(Environment.NewLine, timeDimensions4));
            schema.Replace("<!--{DimensionLinks}-->", String.Join(Environment.NewLine, dimensionLinks4.Union(timeDimensionLinks4)));
            schema.Replace("<!--{CalculatedMeasures}-->", String.Join(Environment.NewLine, calculatedMeasures4));
            ViewData["Schema"] = schema.ToString();
            return View("Mondrian");
        }

        private string GetAggregation(string name)
        {
            string aggregation = string.Empty;
            if (string.IsNullOrEmpty(name))
            {
                aggregation = "";
            }
            if (name.ToLower().Contains("sum"))
            {
                aggregation = "sum";
            }
            if (name.ToLower().Contains("average"))
            {
                aggregation = "avg";
            }
            if (name.ToLower().Contains("count"))
            {
                aggregation = "count";
            }
            if (name.ToLower().Contains("distinct"))
            {
                aggregation = "distinct-count";
            }
            return aggregation;
        }


        private static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        private static byte[] Zip(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    //msi.CopyTo(gs);
                    CopyTo(msi, gs);
                }

                return mso.ToArray();
            }
        }

        private static string Unzip(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    //gs.CopyTo(mso);
                    CopyTo(gs, mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }

        private MemoryStream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// Convert SVG to File
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost("Image")]
        public ActionResult WidgetImage(string data)
        {
            string decodedString = Unzip(Convert.FromBase64String(data));
            decodedString = "<?xml version=\"1.0\"?><!DOCTYPE svg PUBLIC \"-//W3C//DTD SVG 1.1//EN\" \"http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd\">" + decodedString;
            //var filename = Path.GetTempFileName();
            //System.IO.File.WriteAllText(filename, decodedString);
            //var svgDocument = Svg.SvgDocument.Open(filename);
            //var bitmap = svgDocument.Draw();
            //var ms = GenerateStreamFromString(decodedString);
            //bitmap.Save(ms, ImageFormat.Png);

            var result = new FileContentResult(Encoding.UTF8.GetBytes(decodedString), "image/svg")
            {
                FileDownloadName = "WidgetImage.svg"
            };
            return result;
        }

    }


    /// <summary>
    /// Chart Viewer
    /// </summary>
    [Route("Charts")]
    public class ChartsController : BaseController
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        public ChartsController(ITenantContext context) : base(context)
        {

        }
        /// <summary>
        /// Chart Viewer for Embedding
        /// </summary>
        /// <param name="tenant"></param>
        /// <param name="shortCode"></param>
        /// <returns></returns>
        [HttpGet("{tenant}/{shortCode}")]
        public ActionResult ChartViewer(string tenant, string shortCode)
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(shortCode)).Split('|');
            string widgetId = decoded[0];
            long progression = Convert.ToInt64(decoded[1]);
            ViewBag.tenant = tenant;
            ViewBag.shortCode = shortCode;
            ViewBag.widgetId = widgetId;
            ViewBag.progression = progression;
            return View();

        }



        /// <summary>
        /// Chart View For Image
        /// </summary>
        /// <param name="tenant"></param>
        /// <param name="id"></param>
        /// <param name="progression"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{tenant}/{id}/{progression}/View")]
        public IActionResult ChartViewForImage(string tenant, long id, long progression, CancellationToken cancellationToken = default(CancellationToken))
        {
            ViewBag.tenant = tenant;
            ViewBag.shortCode = "";
            ViewBag.widgetId = id.ToString();
            ViewBag.progression = progression;
            return View("ChartViewer");
        }

    }
}
