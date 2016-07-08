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
using System.Threading;
using System.Threading.Tasks;
using Infoveave.Data.Interfaces;
using Infoveave.Models;
using Microsoft.EntityFrameworkCore;

namespace Infoveave.Data.Implementations
{
    public class DataUow : IDataUow
    {
        protected DbContext DbContext { get; private set; }
        protected Dictionary<Type, object> Repositories { get; set; }

        protected IRepository<T> GetStandardRepo<T>() where T : class
        {
            if (Repositories.ContainsKey(typeof(T)))
            {
                return (IRepository<T>)Repositories[typeof(T)];
            }
            return MakeRepo<T>();
        }

        protected IRepository<T> MakeRepo<T>() where T : class
        {
            var dataRepo = new DataRepository<T>(this.DbContext);
            Repositories[typeof(T)] = dataRepo;
            return dataRepo;
        }

        public DataUow(DbContextOptionsBuilder dbContextOptions)
        {
            Repositories = new Dictionary<Type, object>();
            var context = new DataContext(dbContextOptions.Options);
            context.Database.Migrate();
            DbContext = context;
        }
        public IRepository<Organisation> Organisations { get { return GetStandardRepo<Organisation>(); } }
        public IRepository<Analysis> Analysis { get { return GetStandardRepo<Analysis>(); } }
        public IRepository<AnalysisDimensionItem> AnalysisDimensionItems { get { return GetStandardRepo<AnalysisDimensionItem>(); } }
        public IRepository<AnalysisDimension> AnalysisDimensions { get { return GetStandardRepo<AnalysisDimension>(); } }
        public IRepository<AnalysisScenario> AnalysisScenarios { get { return GetStandardRepo<AnalysisScenario>(); } }
        public IRepository<DataSource> DataSources { get { return GetStandardRepo<DataSource>(); } }
        public IRepository<Dimension> Dimensions { get { return GetStandardRepo<Dimension>(); } }
        public IRepository<Formula> Formula { get { return GetStandardRepo<Formula>(); } }
        public IRepository<FormulaElementDimension> FormulaElementDimensions { get { return GetStandardRepo<FormulaElementDimension>(); } }
        public IRepository<FormulaElement> FormulaElements { get { return GetStandardRepo<FormulaElement>(); } }
        public IRepository<InfoboardLink> InfoboardItems { get { return GetStandardRepo<InfoboardLink>(); } }
        public IRepository<Infoboard> Infoboards { get { return GetStandardRepo<Infoboard>(); } }
        public IRepository<Measure> Measures { get { return GetStandardRepo<Measure>(); } }
        public IRepository<Reports> Reports { get { return GetStandardRepo<Reports>(); } }
        public IRepository<UserRole> Roles { get { return GetStandardRepo<UserRole>(); } }
        public IRepository<User> Users { get { return GetStandardRepo<User>(); } }
        public IRepository<UserRole> UserRoles { get { return GetStandardRepo<UserRole>(); } }
        public IRepository<Widget> Widgets { get { return GetStandardRepo<Widget>(); } }
        public IRepository<UploadBatch> UploadBatches { get { return GetStandardRepo<UploadBatch>(); } }
        public IRepository<DataReports> DataReports { get { return GetStandardRepo<DataReports>(); } }

        public IRepository<WidgetAnnotation> WidgetAnnotations { get { return GetStandardRepo<WidgetAnnotation>(); } }
        public Task<int> CommitAsync()
        {
            return DbContext.SaveChangesAsync();
        }
        public Task<int> CommitAsync(CancellationToken cancellationToken)
        {
            return DbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
