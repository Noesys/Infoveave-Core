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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Infoveave.AdapterFramework;

namespace Infoveave.CacheProvider
{
    public interface ICacheProvider
    {
        /// <summary>
        /// This Method Should be Called Only Once During 
        /// Application Initialisation.
        /// </summary>
        /// <param name="enabled"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        void ConfigureCaching(bool enabled, string host, int port);


        #region "OLAP Adapter Related"

        Task<ExecuteQueryResult> ExecuteQueryAsync(IOlapAdapter adapter, OlapConnection connection, AnalysisQuery mdxquery, CancellationToken cancellationToken = default(CancellationToken));
        Task<List<string>> GetDimesnionItemsAsync(IOlapAdapter adapter, OlapConnection connection, string query, string filter = null, CancellationToken cancellationToken = default(CancellationToken));
        Task RefreshDataSourceAsync(IOlapAdapter adapter, OlapConnection connection);

        #endregion

       


        #region "SQL Adapter Related"

        Task<KeyValuePair<bool, List<Dictionary<string, dynamic>>>> ExecuteQueryAsync(ISQLAdapter adapter, SQLConnection connectionString, string query);
        Task<KeyValuePair<bool,Dictionary<string, string>>> GetColumnsAsync(ISQLAdapter adapter, SQLConnection connectionString, string table);
        Task<KeyValuePair<bool,IList<string>>> GetTablesAsync(ISQLAdapter adapter, SQLConnection connectionString);

        #endregion

    }
}