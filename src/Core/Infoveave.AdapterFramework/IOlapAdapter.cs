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
using System.Threading.Tasks;

namespace Infoveave.AdapterFramework
{
    public interface IOlapAdapter
    {
        string Provider {get;}
        bool SupportsDatabaseList { get; }
        bool SupportsCache { get; }
        Task<KeyValuePair<bool, string>> TestConnectivityAsync(OlapConnection connection);
        Task<List<string>> GetDatabasesAsync(OlapConnection connection);
        Task<List<string>> GetCubesAsync(OlapConnection connection);
        Task<Dictionary<string, string>> GetMeasuresAsync(OlapConnection connection);
        Task<List<OlapDimensionGroup>> GetDimensionsAsync(OlapConnection connection);
        Task<ExecuteQueryResult> ExecuteQueryAsync(OlapConnection connection,AnalysisQuery query);
        Task<List<string>> GetDimensionItemsAsync(OlapConnection connection, string hierarcy, string filter = null);
        Task<bool> FlushCache(OlapConnection connection);

    }

    public class ExecuteQueryResult
    {
        public bool FromCache { get; set; }
        public AnalysisQuery Query { get; set; }
        public List<List<dynamic>> Result { get; set; }
    }

    public class OlapConnection
    {
        public string Server { get; set; }
        public string Database { get; set; }
        public string Cube { get; set; }
        public dynamic AdditionalData { get; set; }
    }

    public class OlapDimensionGroup
    {
        public string Caption { get; set; }
        public string UniqueName { get; set; }
        public string DimensionName { get; set; }
        public string HierarchyName { get; set; }
        public bool Date { get; set; }
    }
}
