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
using Infoveave.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Infoveave.Data
{
    public class DataContext : DbContext
    {
        public DataContext()
        {

        }
        public DataContext(DbContextOptions options) : base(options)
        {

        }
        public DbSet<Organisation> Organisations { get; set; }
        public DbSet<DataSource> DataSources { get; set; }
        public DbSet<Measure> Measures { get; set; }
        public DbSet<Dimension> Dimensions { get; set; }
        public DbSet<Infoboard> Infoboards { get; set; }
        public DbSet<InfoboardLink> InfoboardLinks { get; set; }
        public DbSet<Widget> Widgets { get; set; }
        public DbSet<Formula> Formulae { get; set; }
        public DbSet<FormulaElement> FormulaElements { get; set; }
        public DbSet<FormulaElementDimension> FormulaElementDimensions { get; set; }
        public DbSet<Analysis> Analysis { get; set; }
        public DbSet<AnalysisDimension> AnalysisDimensions { get; set; }
        public DbSet<AnalysisDimensionItem> AnalysisDimensionItems { get; set; }
        public DbSet<AnalysisScenario> AnalysisScenarios { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<UploadBatch> UploadBatches { get; set; }
        public DbSet<Reports> Reports { get; set; }
        public DbSet<DataReports> DataReports { get; set; }

        public DbSet<WidgetAnnotation> WidgetAnnotations { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#if !NET461
                // optionsBuilder.UseNpgsql("Host=localhost;Username=infoveave;Password=infoveave$321;Database=infoveave");
                optionsBuilder.UseSqlServer("Server=(localdb)\v12.0;Integrated Security=true;MultipleActiveResultSets=true;");
#endif

#if NET461
                optionsBuilder.UseSqlCe(@"Data Source=C:\Projects\Infoveave\Data\Infoveave.sdf");
#endif
            }

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.Relational().TableName = entity.DisplayName();
            }
            base.OnModelCreating(modelBuilder);
        }
    }
}
