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
using System.ComponentModel.DataAnnotations;
using System.Linq;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Infoveave.ViewModels
{
    public class AuthenticationResult
    {
        public string Access_token { get; set; }
        public int Expires_in { get; set; }
        public string Token_type { get; set; }
    }

    public class LoginRequest
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Grant_type { get; set; }
        [Required]
        public string Scope { get; set; }
        [Required]
        [RegularExpression("tenant:[A-Za-z0-9]*")]
        public string Acr_values { get; set; }
    }

    public class Infoboard
    {
        public long? Id { get; set; }
        public string Name { get; set; }
        public string ShortCode { get; set; }
        public string Options { get; set; }
        public long? SortOrder { get; set; } 
        public string Layouts { get; set; }
    }

    public class InfoboardMeasure
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Query { get; set; }
        public long DataSourceId { get; set; }
    }

    public class InfoboardDimension
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Query { get; set; }
        public bool IsDate { get; set; }
        public long DataSourceId { get; set; }
    }

    public class InfoboardDisplay : Infoboard
    {
        public IEnumerable<InfoboardItem> Items { get; set; }
        public IEnumerable<InfoboardMeasure> Measures { get; set; }
        public IEnumerable<InfoboardDimension> Dimensions { get; set; }
    }


    public class InfoboardItem
    {
        public long Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int W { get; set; }
        public int H { get; set; }
        public long WidgetId { get; set; }
    }

    public class Widget
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Data { get; set; }
        public string DataSourceIds { get; set; }
        public string ShortCode { get; set; }
        public bool IsPublic { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    sealed public class FormulaElementTypeAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var valid = new string[] { "Measure", "Value", "Expression" };
            return valid.Contains(value.ToString());
        }
    }

    public class DataSource
    {
        public long Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }

        public IEnumerable<Measure> Measures { get; set; }
        public IEnumerable<Dimension> Dimensions { get; set; } 
        public List<ColumnMapping> ColumnMapping { get; set; }
        public string ValidationSchema { get; set; }
        public bool? IsPublic { get; set; }
        public bool? CanShare { get; set; }
    }


    public class TableAndColumnMappings
    {
        public long DataSourceId { get; set; }
        public string TableName { get; set; }
        public List<ColumnMapping> ColumnMapping { get; set; }
    }

    public class Measure
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Query { get; set; }

        public long? DataSourceId { get; set; }
    }

    public class Dimension
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Query { get; set; }
        public bool IsDate { get; set; }

        public long? DataSourceId { get; set; }
    }
    
    public class DimensionItem
    {
        public long Id { get; set; }
        public long DimensionId { get; set; }
        public string DimensionName { get; set; }
        public string MdxName { get; set; }
    }



    public class FileMeasure
    {
        public string Name { get; set; }
        public string ColumnName { get; set; }
        public string Aggregator { get; set; }
        public string DataQuery { get; set; }
        public string Uom { get; set; }
        public bool IsPrefix { get; set; }
    }

    public class FileDimension
    {
        public string Name { get; set; }
        public string ColumnName { get; set; }
        public bool IsDate { get; set; }
        public bool IsRangedimension { get; set; }
        public string RangeDimension { get; set; }
    }

    public class ColumnMapping
    {
        public bool IsComputed { get; set; }
        public string SourceName { get; set; }
        public string DisplayName { get; set; }
        public string TableName { get; set; }
        public string Type { get; set; }
        public bool IsMandatory { get; set; }
        public string Format { get; set; }

    }

    public class FileBasedSource
    {
        public string Name { get; set; }
        public string ValidationSchema { get; set; }

        public List<ColumnMapping> ColumnMappings { get; set; }
        public List<FileMeasure> Measures { get; set; }
        public List<FileDimension> Dimensions { get; set; }
    }



    
    public class InputDataSource
    {
        public string Adapter { get; set; }
        public string Server { get; set; }
        public string Database { get; set; }
        public string Cube { get; set; }
    }


    public class User
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsLocked { get; set; }
        public long RoleId { get; set; }
        public string ImagePath { get; set; }

        public string Tenant { get; set; }

        public string Language { get; set; }

        public List<Models.UserContext> UserContext { get; set; }

        public string Version { get; set; }

        public string NewPassword { get; set; }
        public bool? ImportDashboards { get; set; }
    }

    public class AppUser
    {
        public string Email { get; set; }
    }

    public class Role
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string CreatedBy { get; set; }    
        public DateTime CreatedOn { get; set; }
        public string Permissions { get; set; }
    }
    public class View
    {
        public string view { get; set; }
        public string displayName { get; set; }
        public string[] actions { get; set; }
    }
    public class Permission
    {
        public string module { get; set; }
        public string displayName { get; set; }
        public string displayPath { get; set; }
        public List<View> views { get; set; }
    }
    public class Report
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        public string CreatedByUserId { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ScheduleReport { get; set; }
        public string MailTo { get; set; }
        public long? ReportId { get; set; }
        public string CreatedBy { get; set; }
        public string Type { get; set; }
        public dynamic Parameter { get; set; }
        public long OrganisationId { get; set; }
        public dynamic ScheduleParameter { get; set; }
        public long? DataSourceId { get; set; }
    }


    public class ReportSchedule
    {
        public string Schedule { get; set; }
        public string Parameters { get; set; }
        public string ParamString { get; set; }
        public bool SelectAll { get; set; }
        public bool AddressBook { get; set; }
        public string MapWith { get; set; }
        public string Recipients { get; set; }
        public Dictionary<string, string> QueryParameters { get; set; }
    }

    public class ReportScheduleInfo
    {
        public string ScheduleString { get; set; }
        public List<string> Recipients { get; set; }
        public List<JsonSQLWhereField> Parameters { get; set; }
    }
  
   

    public class UploadImage
    {
        public string name { get; set; }
        public string path { get; set; }
    }
    public class ChangePassword
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }



    public class JsonSQLField
    {
        public string Label { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }
    }

    public class JsonSQLSelectField
    {
        public string Function { get; set; }
        public string Field { get; set; }
        public string Display { get; set; }
    }

    public class JsonSQLSortField
    {
        public JsonSQLField Field { get; set; }
        public string Sort { get; set; }
    }

    public class JsonSQLWhereField
    {
        public string Comparer { get; set; }
        public JsonSQLField Field { get; set; }
        public dynamic Expression { get; set; }
    }

    public class JsonSQLWhereSet
    {
        public string Comparer { get; set; }
        public List<JsonSQLWhereField> Fields { get; set; }
        public List<JsonSQLWhereSet> Sets { get; set; }
    }

    public class JsonSQLQuery
    {
        public List<JsonSQLSelectField> SelectFields { get; set; }
        public List<JsonSQLField> GroupByFields { get; set; }
        public List<JsonSQLSortField> OrderByFields { get; set; }
        public JsonSQLWhereSet WhereFields { get; set; }

        public List<JsonSQLWhereField> Parameters { get; set; }

    }
    public class ManualSQLQuery
    {
        public string Query { get; set; }
        public List<JsonSQLWhereField> Parameters { get; set; }

    }
    public class DataReport
    {
        public long Id { get; set; }
        public string Name { get; set; }     
        public string CreatedByUserId { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ScheduleReport { get; set; }      
        public string CreatedBy { get; set; }
        public List<JsonSQLWhereField> Parameter { get; set; }
        public string ScheduleParameter { get; set; }
        public long DataSourceId { get; set; }
        public string Query { get; set; }
        public JsonSQLQuery JsonSQLQuery { get; set; }
    }


    public class CalculatedMeasure
    {
        public long? Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Query { get; set; }
        public string Formula { get; set; }
        public string Prefix { get; set; }
        public string Suffix { get; set; }

        public bool IsPercent { get; set; }

    }

    public class MondrianDirectMeasure
    {
        public string Name { get; set; }
        public string Query { get; set; }

        public bool IsPrefix { get; set; }
        public string Uom { get; set; }
        public bool IsCummulativeMeasure { get; set; }

    }

    public class MondrianDirectDimensions {
        public string Name { get; set; }
        public string Query { get; set; }
        public bool IsDate { get; set; }
    }
    public class MondrianDirectDataSource : InputDataSource
    {
        public string FileName { get; set; }
    }


    public class MondrianDirectDataSourceWithMesuresAndDimensions : MondrianDirectDataSource
    {
        public string Name { get; set; }

        public List<MondrianDirectMeasure> Measures { get; set; }

        public List<MondrianDirectDimensions> Dimensions { get; set; }

    }


    
    public class ColorPaletteMap
    {
        public int DataSourceId { get; set; }
        public int DimensionId { get; set; }
        public string DimensionName { get; set; }
        public string DimensionItem { get; set; }
        public string Color { get; set; }
    }


    public class WidgetRequestMeasure
    {
        public long DataSourceId { get; set; }
        public long Id { get; set; }
        public string Query { get; set; }

    }

    public class WidgetRequestDimension
    {
        public string Query { get; set; }

        public string Operator { get; set; }
        public List<string> Items { get; set; }
    }

    public class WidgetRequestFilter
    {
        public string Query { get; set; }
        public string Operator { get; set; }
        public List<string> Items { get; set; }
    }

    public class WidgetRequest
    {
        /// <summary>
        /// Measures to retrieve
        /// </summary>
        public List<WidgetRequestMeasure> Measures { get; set; }
        /// <summary>
        /// Dimensions to retrieve
        /// </summary>
        public List<WidgetRequestDimension> Dimensions { get; set; }
        /// <summary>
        /// Filters to Apply
        /// </summary>
        public List<WidgetRequestFilter> Filters { get; set; }
        /// <summary>
        /// Custom Date Query
        /// </summary>
        public string DateQuery { get; set; }

        /// <summary>
        /// Start Date
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End Date
        /// </summary>
        public DateTime EndDate { get; set; }


        /// <summary>
        /// Retrieve Null Values
        /// </summary>
        public bool RetrieveNullValues { get; set; }
    }

    public class WidgetResponseMeasureMetadata
    {
        /// <summary>
        /// Internal Query
        /// </summary>
        public string Query { get; set; }
        public bool IsPercent { get; set; }
        public string Prefix { get; set; }
        public string Suffix { get; set; }
    }


    public class WidgetReponse
    {
        /// <summary>
        /// Data Retrieved from Cache or Source
        /// </summary>
        public List<List<dynamic>> Data { get; set; }
        /// <summary>
        /// Metadata for the measures retrived
        /// </summary>
        public IEnumerable<WidgetResponseMeasureMetadata> MeasureMetaData { get; set; }
        /// <summary>
        /// 1: Cache
        /// 2: Source
        /// </summary>
        public int DataFetchedFrom { get; set; }
        /// <summary>
        /// Execution time in (ms)
        /// </summary>
        public long ExecutionTime { get; set; }

        /// <summary>
        /// Date Interval Which was Used in the Request 
        /// It would be [Year],[Quarter],[Month],[Week],[Day]
        /// </summary>
        public string DateIntervalUsed { get; set; }
    }

    public class WidgetAnnotation
    {
        public long? Id { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Data { get; set; }
        public string Content { get; set; }
        public DateTime CreatedOn { get; set; }
        public long CreatedBy { get; set; }
        public string CreatedByUser { get; set; }
    }

    public class SchemaResult
    {
        public string FileName { get; set; }
        public string AnalysisDatabase { get; set; }
        public string[] CubeNames { get; set; }
    }

    public class MondrianDirectMeasuresAndDimensions
    {
        public List<Measure> Measures { get; set; }
        public List<Dimension> Dimensions { get; set; }
    }

    public class SQLAndPreviewResult
    {
        public string Query { get; set; }
        public string Result { get; set; }
        public int TotalRowCount { get; set; }
    }


    public class UploadBatchInfo
    {
        public long BatchId { get; set; }
        public long NoOfRows { get; set; }
        public DateTime UploadTime { get; set; }
    }

    public class InfoboardLayout
    {
        public List<InfoboardLayoutWidget> lg { get; set; }
        public List<InfoboardLayoutWidget> md { get; set; }
        public List<InfoboardLayoutWidget> sm { get; set; }
    }

    public class InfoboardLayoutWidget
    {
        public int x { get; set; }
        public int y { get; set; }
        public int w { get; set; }
        public int h { get; set; }
        public int i { get; set; }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member