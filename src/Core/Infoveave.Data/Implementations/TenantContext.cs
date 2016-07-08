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
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Infoveave.Data.Implementations
{
    /// <summary>
    /// Tenant Based DataRepositories, Each Teanant is allocated its own database,
    /// Meant to be used in Singleton Pattern,
    /// </summary>
    public class TenantContext : ITenantContext
    {
        protected Dictionary<string, IDataUow> TenantRepositories { get; private set; }

        protected bool UseMultiTenancy { get; private set; }
        protected string BaseConnectionString { get; private set; }
        public TenantContext()
        {
            TenantRepositories = new Dictionary<string, IDataUow>();
        }
        /// <summary>
        /// Set the Database Connection to the MetadataStore
        /// </summary>
        /// <param name="useMultiTenancy">Should we Use Multi tenancy</param>
        /// <param name="baseConnectionString">Base ConnectionString </param>
        public void SetDatabaseConnection(bool useMultiTenancy, string baseConnectionString)
        {
            UseMultiTenancy = useMultiTenancy;
            BaseConnectionString = baseConnectionString;
        }

        public IDataUow GetTenantRepository(string tenant)
        {
            if (!TenantExists(tenant)) { throw new Exception("TNT-002", new Exception("Tenant not found/registered")); }
            //if (TenantRepositories.ContainsKey(tenant))
            //{
            //    return TenantRepositories[tenant];
            //}
            return MakeTenantRepository(tenant);
        }

        private IDataUow MakeTenantRepository(string tenant)
        {
            var dbContextOptions = new DbContextOptionsBuilder();

            if (UseMultiTenancy)
            {
#if NET461               
                DirectoryInfo di = Directory.CreateDirectory($@"{BaseConnectionString}\{tenant}");
                dbContextOptions.UseSqlCe($@"Data Source={BaseConnectionString}\{tenant}\{tenant}.sdf");
#endif
            }
            else
            {
                // dbContextOptions.UseNpgsql(BaseConnectionString);
                dbContextOptions.UseSqlServer(BaseConnectionString);
            }
            var tenantRepository = new DataUow(dbContextOptions);
            //TenantRepositories[tenant] = tenantRepository;
            return tenantRepository;

        }

        public bool TenantExists(string tenant)
        {
            if (!UseMultiTenancy) return true;
            return System.IO.File.Exists($@"{BaseConnectionString}\{tenant}\{tenant}.sdf");
        }

        public IDataUow CreateTenantRepository(string tenant)
        {
            return MakeTenantRepository(tenant);
        }
    }
}
