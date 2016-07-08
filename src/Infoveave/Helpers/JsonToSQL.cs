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
using Infoveave.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infoveave.Helpers
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class JsonToSQL
    {
        public static string GenerateSQL(JsonSQLQuery query, string table, string type)
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append("Select ");
            var selectFields = query.SelectFields.Select(s => SelectBuilder(s, type));
            selectFields = selectFields.Where(s => s != null);
            sqlQuery.Append(string.Join(", ", selectFields));
            sqlQuery.Append(" from " + WrapField(table, type));


            var groupByFields = query.GroupByFields.Select(s => GroupByBuilder(s, type));
            groupByFields = groupByFields.Where(s => s != null);


            var whereField = WhereBuilder(query.WhereFields, query.Parameters, type);
            if (!string.IsNullOrEmpty(whereField))
            {
                sqlQuery.Append(" where " + whereField);
            }

            if (groupByFields.Count() > 0)
            {
                sqlQuery.Append(" group by " + string.Join(",", groupByFields));
            }

            var orderByFields = query.OrderByFields.Select(s => OrderByBuilder(s, type));
            orderByFields = orderByFields.Where(s => s != null);
            if (orderByFields.Count() > 0)
            {
                sqlQuery.Append(" order by " + string.Join(",", orderByFields));
            }
            return sqlQuery.ToString();

        }

        protected static string SelectBuilder(JsonSQLSelectField sqlField, string type)
        {
            if (string.IsNullOrEmpty(sqlField.Field))
            {
                return null;
            }
            if (string.IsNullOrEmpty(sqlField.Function))
            {
                return string.Format("{0} as {1}", WrapField(sqlField.Field, type),  WrapField(sqlField.Display, type));
            }
            else
            {
                return string.Format("{0}({1}) as {2}", sqlField.Function, WrapField(sqlField.Field, type), WrapField(sqlField.Display,type));
            }
        }

        protected static string OrderByBuilder(JsonSQLSortField field, string type)
        {

            if (field == null || field.Field == null || string.IsNullOrEmpty(field.Field.Value))
            {
                return null;
            }
            return string.Format("{0} {1}", WrapField(field.Field.Value, type), field.Sort);
        }

        protected static string GroupByBuilder(JsonSQLField field, string type)
        {
            if (field == null || string.IsNullOrEmpty(field.Value))
            {
                return null;
            }
            return WrapField(field.Value, type);
        }


        protected static string WhereBuilder(JsonSQLWhereSet set, List<JsonSQLWhereField> parameters, string type)
        {
            var fields = set.Fields.Select(s => WhereFieldBuilder(s, parameters, type));
            fields = fields.Where(f => f != null);
            var subsets = set.Sets.Select(s => WhereBuilder(s, parameters, type));
            subsets = subsets.Where(s => s != null);
            var fieldString = ""; var subsetString = "";
            if (fields.Count() > 0)
            {
                fieldString = string.Join(" " + set.Comparer + " ", fields);
            }
            if (subsets.Count() > 0)
            {
                subsetString = string.Join(" " + set.Comparer + " ", subsets.Select(s => " ( " + s + " ) "));
            }
            if (fieldString == "" && subsetString == "") return null;
            if (fieldString != "" && subsetString != "") return fieldString + " " + set.Comparer + " " + subsetString;
            if (fieldString != "") return fieldString;
            if (subsetString != "") return subsetString;
            return null;
        }


        protected static string WhereFieldBuilder(JsonSQLWhereField field, List<JsonSQLWhereField> parameters, string type)
        {
            if (field == null || field.Field == null || field.Field.Value == null)
            {
                return null;
            }
            string comparer = "";
            if (field.Comparer == "equal") comparer = "=";
            if (field.Comparer == "notequal") comparer = "!=";
            if (field.Comparer == "greaterThan") comparer = ">";
            if (field.Comparer == "greaterThanEquals") comparer = ">=";
            if (field.Comparer == "lessThan") comparer = "<";
            if (field.Comparer == "lessThanEquals") comparer = "<=";
            if (field.Comparer == "in") comparer = "in";
            if (field.Comparer == "notin") comparer = "not in";
            if (field.Comparer == "like") comparer = "like";
            if (field.Comparer == "isNull") comparer = "is null";
            if (field.Comparer == "isNotNull") comparer = "is Not Null";
            if (field.Comparer == "between") comparer = "between";
            if (field.Comparer == "parameter" && field.Field.Type != "Date")
            {
                comparer = "in";
                var inParam = parameters.FirstOrDefault(f => f.Field.Value == field.Field.Value);
                field.Comparer = (inParam != null) ? "in" : null;
                field.Expression = (inParam != null) ? inParam.Expression : null;
            }
            if (field.Comparer == "parameter" && field.Field.Type == "Date")
            {
                comparer = "between";
                var inParam = parameters.FirstOrDefault(f => f.Field.Value == field.Field.Value);
                field.Comparer = (inParam != null) ? "between" : null;
                field.Expression = (inParam != null) ? inParam.Expression : null;
            }
            string value = "";
            if (field.Comparer == "greaterThan" || field.Comparer == "greaterThanEquals" || field.Comparer == "lessThan" || field.Comparer == "lessThanEquals")
            {
                value = field.Expression;
            }
            if (field.Comparer == "equal" || field.Comparer == "notequal")
            {
                value = field.Expression.value;
                if (field.Field.Type == "Text") value = QuoteString(value);
            }
            if (field.Comparer == "in" || field.Comparer == "notin")
            {
                List<String> values = new List<string>();
                foreach (var item in field.Expression)
                {
                    values.Add(item.value.ToString());
                }
                if (field.Field.Type == "Text")
                {
                    values = values.Select(v => QuoteString(v)).ToList();
                }
                value = "( " + string.Join(", ", values) + " )";
                if (value == "(  )") value = "";
            }
            if (field.Comparer == "between")
            {
                int progression = 0;
                var isProgression = int.TryParse(field.Expression.ToString(), out progression);
                DateTime from, to;
                if (isProgression)
                {
                    var startEndDate = Helpers.DateHelpers.GetStateEndDate(progression);
                    from = startEndDate.StartDate;
                    to = startEndDate.EndDate;
                }
                else
                {
                    from = Convert.ToDateTime(field.Expression[0].ToString());
                    to = Convert.ToDateTime(field.Expression[1].ToString());

                }
                value = " '" + from.ToString("yyyy-MM-dd 00:00:00") + "' and '" + to.ToString("yyyy-MM-dd 00:00:00") + "'";
            }
            if (field.Comparer != "isNull" && field.Comparer != "isNotNull" && value == "")
            {
                return null;
            }

            return "(" + WrapField(field.Field.Value, type) + " " + comparer + " " + value + ")";
        }

        protected static string QuoteString(string field)
        {
            return string.Format("'{0}'", field);
        }
        public static string WrapField(string field, string type)
        {
            if (type == "sqlite" || type == "mssql")
            {
                return string.Format("[{0}]", field);
            }
            if (type == "mysql")
            {
                return string.Format("`{0}`", field);
            }
            if (type == "pgsql")
            {
                return string.Format("\"{0}\"", field);
            }
            else return field;
        }
        public static string ManualWhereFieldBuilder(JsonSQLWhereField field, List<JsonSQLWhereField> parameters, string type)
        {
            if (field == null || field.Field == null || field.Field.Value == null)
            {
                return null;
            }
            /*string comparer = "";
            if (field.Comparer == "equal") comparer = "=";
            if (field.Comparer == "notequal") comparer = "!=";
            if (field.Comparer == "greaterThan") comparer = ">";
            if (field.Comparer == "greaterThanEquals") comparer = ">=";
            if (field.Comparer == "lessThan") comparer = "<";
            if (field.Comparer == "lessThanEquals") comparer = "<=";
            if (field.Comparer == "in") comparer = "in";
            if (field.Comparer == "notin") comparer = "not in";
            if (field.Comparer == "like") comparer = "like";
            if (field.Comparer == "isNull") comparer = "is null";
            if (field.Comparer == "isNotNull") comparer = "is Not Null";
            if (field.Comparer == "between") comparer = "between";*/
            if (field.Comparer == "parameter" && field.Field.Type != "Date")
            {
                // comparer = "in";
                var inParam = parameters.FirstOrDefault(f => f.Field.Value == field.Field.Value);
                field.Comparer = (inParam != null) ? "in" : null;
                field.Expression = (inParam != null) ? inParam.Expression : null;
            }
            if (field.Comparer == "parameter" && field.Field.Type == "Date")
            {
                var inParam = parameters.FirstOrDefault(f => f.Field.Value == field.Field.Value);
                field.Comparer = (inParam != null) ? "between" : null;
                field.Expression = (inParam != null) ? inParam.Expression : null;
            }
            string value = "";
            if (field.Comparer == "greaterThan" || field.Comparer == "greaterThanEquals" || field.Comparer == "lessThan" || field.Comparer == "lessThanEquals")
            {
                value = field.Expression;
            }
            if (field.Comparer == "equal" || field.Comparer == "notequal")
            {
                value = field.Expression.value;
                if (field.Field.Type == "Text") value = QuoteString(value);
            }
            if (field.Comparer == "in" || field.Comparer == "notin")
            {
                List<String> values = new List<string>();
                foreach (var item in field.Expression)
                {
                    values.Add(item.value.ToString());
                }
                if (field.Field.Type == "Text")
                {
                    values = values.Select(v => QuoteString(v)).ToList();
                }
                value = "( '" + string.Join("', '", values) + "' )";
                if (value == "(  )") value = "";
            }
            if (field.Comparer == "between")
            {
                DateTime from = Convert.ToDateTime(field.Expression[0].ToString());
                DateTime to = Convert.ToDateTime(field.Expression[1].ToString());
                value = " '" + from.ToString("yyyy-mm-dd 00:00:00") + "' and '" + to.ToString("yyyy-mm-dd 00:00:00") + "'";
            }
            if (field.Comparer != "isNull" && field.Comparer != "isNotNull" && value == "")
            {
                return null;
            }

            return value;
        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
