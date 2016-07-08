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
using System.Text;
using System.Threading.Tasks;

namespace Infoveave.AdapterFramework
{
    public static class Extensions
    {
        private static readonly List<string> _TopsAndBottoms = new List<string> { "Top5", "Bottom5", "Top10", "Bottom10", "Top20", "Bottom20" };
        public static string GetQuery(this AnalysisQuery query)
        {
            var queryString = new StringBuilder();
            if (query.Measures.Count == 0) throw new Exception("No Measures in the Query");

            if (query.WithMembers.Count > 0)
            {
                queryString.AppendLine("With");
                queryString.AppendLine(WithClauseBuilder(query));
            }
            queryString.AppendLine(String.Format("Select {{ {0} }} on Columns", String.Join(", ", query.Measures.ToArray())));
            if (query.Dimensions.Count > 0)
            {
                queryString.AppendLine(String.Format(" , {0} On Rows", AxisBuilder(query.Dimensions, query.RetrieveEmpty, query.Measures.ElementAt(0))));
            }
            queryString.AppendLine(String.Format(" From [{0}]", query.Cube));
            if (query.Filters.Count > 0)
            {
                var result = WhereBuilder(query.Filters);
                if (result.Trim() != "{  }")
                {
                    queryString.AppendLine(String.Format("Where {0}", result));
                }
            }
            queryString.Replace("[Date]", query.CustomDateDimension);
            return queryString.Replace(Environment.NewLine, " ").ToString();
        }

        public static string WithClauseBuilder(AnalysisQuery query)
        {
            var members = query.WithMembers.SelectMany(w => w.Split(Environment.NewLine.ToCharArray())).Distinct();
            var customDate = query.CustomDateDimension.UnWrap();
            var dateInWhere = false;
            var dateItems = new List<string>();
            if (query.Dimensions.ContainsKey("Date"))
            {
                dateItems = query.Dimensions["Date"].Value;
            }
            else
            {
                dateItems = query.Filters["Date"].Value;
                dateInWhere = true;
            }
            var heirarchy = dateItems.ElementAt(0).Split('.')[0].UnWrap();
            var replacedMembers = members.Select(m => ReplaceWithMember(m, dateItems, dateInWhere, customDate, heirarchy));
            return String.Join(Environment.NewLine, replacedMembers.ToArray());
        }

        public static string AxisBuilder(Dictionary<string, KeyValuePair<string,List<string>>> dimensions, bool retreiveEmpty, string measure)
        {
            var axisString = new StringBuilder();
            var sets = dimensions.ToDictionary(d => d.Key, d => SingleDimensionBuilder(d.Key, d.Value, measure));
            var resultString = string.Empty;
            if (dimensions.Values.SelectMany(d => d.Value).Intersect(_TopsAndBottoms).Count() > 0)
            {
                //Atleast 1 dimension has a Top/Bottom Query
                //So we are using the Generate/CrossJoin Mechanism
                var c = sets.Count() - 1;
                if (c == 0)
                {
                    resultString = sets.ElementAt(0).Value;
                }
                while (c > 0)
                {
                    var currentMember = sets.ElementAt(c - 1).Key;
                    if (currentMember == "Date")
                    {
                        var secondMemberKey = dimensions.ElementAt(c - 1).Value.Value.ElementAt(0).Split('.')[0];
                        currentMember = string.Format("[{0}].{1}", currentMember, secondMemberKey);
                    }
                    resultString = string.Format("Generate({0}, {3}Crossjoin({{{1}.CurrentMember}} ,{2}))",
                           sets.ElementAt(c - 1).Value, currentMember,
                           (string.IsNullOrEmpty(resultString)) ? sets.ElementAt(c).Value : resultString,
                           retreiveEmpty ? "" : "NonEmpty"
                       );
                    c--;
                }
            }
            else
            {
                //This means that there is no Tops/Bottoms
                //So Just do a CrossJoin on eachDimension
                resultString = String.Format(" {{ {0} }}", String.Join(" * ", sets.Values));
                if (!retreiveEmpty) resultString = String.Format("Non Empty( {0} )", resultString);
            }

            return resultString;
        }


        public static string WhereBuilder(Dictionary<string, KeyValuePair<string,List<string>>> filters)
        {
            var sets = filters.ToDictionary(d => d.Key, d => SingleDimensionBuilder(d.Key, new KeyValuePair<string, List<string>>(d.Value.Key,d.Value.Value.Except(_TopsAndBottoms).ToList())));
            sets = sets.Where(s => !string.IsNullOrEmpty(s.Value)).ToDictionary(s => s.Key, s=> s.Value);
            var resultString = String.Format(" {{ {0} }}", String.Join(" * ", sets.Values));
            return resultString;
        }


        public static string SingleDimensionBuilder(string dimension, KeyValuePair<string,List<string>> dimensionItems, string measure = null)
        {

            dimension = dimension.Wrap();

            var intersect = dimensionItems.Value.Intersect(_TopsAndBottoms);
            //Adding Ressilence [Date] should logically never have tops & bottms
            //Possible Enhancement : Show me Top 5 monts where measure is highest ?
            if (intersect.Count() > 0 && dimension != "[Date]")
            {
                //Means that there is one of the option in dimension Items so
                //we have to do a Top/Bottom of the selected stuff
                //Also We check if there is an item called children in the list if so,
                //that takes priority in terms of items and we ignore the rest
                var selectedOption = intersect.FirstOrDefault();
                switch (selectedOption)
                {
                    case "Top5":
                        return string.Format("TopCount({0},5,{1})", SingleDimensionHandler(dimension, new KeyValuePair<string, List<string>>(dimensionItems.Key,dimensionItems.Value.Except(_TopsAndBottoms).ToList())), measure);
                    case "Top10":
                        return string.Format("TopCount({0},10,{1})", SingleDimensionHandler(dimension, new KeyValuePair<string, List<string>>(dimensionItems.Key, dimensionItems.Value.Except(_TopsAndBottoms).ToList())), measure);
                    case "Top20":
                        return string.Format("TopCount({0},20,{1})", SingleDimensionHandler(dimension, new KeyValuePair<string, List<string>>(dimensionItems.Key, dimensionItems.Value.Except(_TopsAndBottoms).ToList())), measure);
                    case "Bottom5":
                        return string.Format("BottomCount({0},5,{1})", SingleDimensionHandler(dimension, new KeyValuePair<string, List<string>>(dimensionItems.Key, dimensionItems.Value.Except(_TopsAndBottoms).ToList())), measure);
                    case "Bottom10":
                        return string.Format("BottomCount({0},10,{1})", SingleDimensionHandler(dimension, new KeyValuePair<string, List<string>>(dimensionItems.Key, dimensionItems.Value.Except(_TopsAndBottoms).ToList())), measure);
                    case "Bottom20":
                        return string.Format("BottomCount({0},20,{1})", SingleDimensionHandler(dimension, new KeyValuePair<string, List<string>>(dimensionItems.Key, dimensionItems.Value.Except(_TopsAndBottoms).ToList())), measure);
                }
            }
            //Handle it regularly
            return SingleDimensionHandler(dimension, dimensionItems);

        }

        public static string SingleDimensionHandler(string dimension, KeyValuePair<string,List<string>> dimensionItems)
        {
            var finalString = String.Empty;
            if (dimension == "[Date]" && dimensionItems.Value.Count == 0)
            {
                // This means that Date Does not have filters so return an empty string;
                return finalString;
            }
            if ((dimensionItems.Value.Contains("Children") || dimensionItems.Value.Count == 0) && dimensionItems.Key == "Exactly")
            {
                finalString = $"{dimension}.Children";
            }
            else if (dimensionItems.Key == "Exactly")
            {
                finalString = String.Join(", ", dimensionItems.Value.Select(d => SingleDimensionItemHandler(dimension, d)));
            }
            else if (dimensionItems.Key == "NotExactly")
            {
                var exclusionList = String.Join(", ", dimensionItems.Value.Select(d => SingleDimensionItemHandler(dimension, d)));
                finalString = $"Except( {dimension}.Children, {{ {exclusionList} }} )";
            }
            else if (dimensionItems.Key == "Contains")
            {
                finalString = $"Filter( {dimension}.Children, {dimension}.CurrentMember.Name MATCHES '(?i).*{dimensionItems.Value[0]}.*')";
            }
            else if (dimensionItems.Key == "NotContains")
            {
                finalString = $"Filter( {dimension}.Children, {dimension}.CurrentMember.Name NOT MATCHES '(?i).*{dimensionItems.Value[0]}.*')";
            }
            return $"{{ {finalString} }}";
        }

        public static string SingleDimensionItemHandler(string dimension, string dimensionItem)
        {
            if (dimensionItem.Contains(":"))
            {
                return String.Format("{0}.{1}:{0}.{2}", dimension, dimensionItem.Split(':')[0].Wrap(), dimensionItem.Split(':')[1].Wrap());

            }
            else
            {
                return String.Format("{0}.{1}", dimension, dimensionItem.Wrap());
            }
        }

        public static string Wrap(this string item)
        {
            string format = "[{0}]";
            return (item.Contains("[")) ? item : String.Format(format, item);
        }

        public static string UnWrap(this string item)
        {
            return item.Trim().TrimStart('[').TrimEnd(']');
        }


        public static string ReplaceWithMember(string query, List<string> selectedRange, bool DateInWhere, string key, string heirarchy)
        {
            //1. @DateRange : The Entire Range (No Additional Checks & Balances)
            //2. @DateKey: The Date dimension which is being replaced
            //3. @DateHeirarchy : The User seleted Heirarchy within that range
            //4. @CurrentOrFirstMember : the current member will be created if the date is on Axis(1) else the first memeber of the selected range would be used.
            //5. @CurrentOrAllSelected: if date is on Axis(1) then CurrentMember would be returned else the entire range is used


            //There are two possibilities The logic can vary exrtremely based on weather the date is on an Axis or if the date
            //is in the where clause. This needs to be handled carefully or we might end up with non-execuable mdx most of the time.
            //Even with the that assumtion 


            var currentOrFistMember = (DateInWhere) ? String.Format("[{0}].{1}", key, selectedRange[0]) : String.Format("[{0}].[{1}].[{1}].CurrentMember", key, heirarchy);
            var currentOrAllSelected = (DateInWhere) ? String.Join(", ", selectedRange.Select((s) => String.Format("[{0}].{1}", key, s))) : String.Format("[{0}].[{1}].[{1}].CurrentMember", key, heirarchy);

            var resultQuery = query;
            resultQuery = resultQuery.Replace("@DateKey", key);
            resultQuery = resultQuery.Replace("@DateHeirarchy", heirarchy);
            resultQuery = resultQuery.Replace("@CurrentOrFirstMember", currentOrFistMember);
            resultQuery = resultQuery.Replace("@CurrentOrAllSelected", currentOrAllSelected);
            return resultQuery;

        }
    }
}
