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
using Infoveave.Models;
using System.Threading;

namespace Infoveave.Data.Interfaces
{
    public interface IDataUow
    {
        // Save pending changes to the data store.
        Task<int> CommitAsync();
        Task<int> CommitAsync(CancellationToken cancellationToken);

        // Repositories
        IRepository<Organisation> Organisations { get; }
        IRepository<User> Users { get; }
        IRepository<Measure> Measures { get; }
        IRepository<Dimension> Dimensions { get; }
        IRepository<UserRole> UserRoles { get; }
        IRepository<DataSource> DataSources { get; }
        IRepository<Reports> Reports { get; }
        IRepository<Formula> Formula { get; }
        IRepository<FormulaElement> FormulaElements { get; }
        IRepository<FormulaElementDimension> FormulaElementDimensions { get; }
        IRepository<Analysis> Analysis { get; }
        IRepository<AnalysisDimension> AnalysisDimensions { get; }
        IRepository<AnalysisDimensionItem> AnalysisDimensionItems { get; }
        IRepository<AnalysisScenario> AnalysisScenarios { get; }
        IRepository<Infoboard> Infoboards { get; }
        IRepository<InfoboardLink> InfoboardItems { get; }
        IRepository<Widget> Widgets { get; }
        IRepository<UploadBatch> UploadBatches { get; }
        IRepository<DataReports> DataReports { get; }

        IRepository<WidgetAnnotation> WidgetAnnotations { get; }
    }
}
