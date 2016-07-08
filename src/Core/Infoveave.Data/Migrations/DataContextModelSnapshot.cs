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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Infoveave.Data;

namespace Infoveave.Data.Migrations
{
    [DbContext(typeof(DataContext))]
    partial class DataContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rtm-21431")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Infoveave.Models.Analysis", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("CreatedBy");

                    b.Property<DateTime>("CreatedOn");

                    b.Property<long>("FormulaId");

                    b.Property<bool>("IsPublic");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Analysis");
                });

            modelBuilder.Entity("Infoveave.Models.AnalysisDimension", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("AnalysisId");

                    b.Property<long>("DimensionId");

                    b.Property<bool>("Driven");

                    b.Property<bool>("View");

                    b.HasKey("Id");

                    b.HasIndex("AnalysisId");

                    b.ToTable("AnalysisDimension");
                });

            modelBuilder.Entity("Infoveave.Models.AnalysisDimensionItem", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("AnalysisId");

                    b.Property<long>("DimensionId");

                    b.Property<string>("DimensionName");

                    b.Property<string>("MdxName");

                    b.HasKey("Id");

                    b.HasIndex("AnalysisId");

                    b.ToTable("AnalysisDimensionItem");
                });

            modelBuilder.Entity("Infoveave.Models.AnalysisScenario", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AnalysisData");

                    b.Property<long>("AnalysisId");

                    b.Property<long>("CreatedBy");

                    b.Property<DateTime>("CreatedOn");

                    b.Property<bool>("IsPublic");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.HasIndex("AnalysisId");

                    b.ToTable("AnalysisScenario");
                });

            modelBuilder.Entity("Infoveave.Models.DataReports", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ConnectionString");

                    b.Property<long>("CreatedBy");

                    b.Property<DateTime>("CreatedOn");

                    b.Property<long>("DataSourceId");

                    b.Property<string>("DataStructure");

                    b.Property<string>("MailTo");

                    b.Property<string>("Name");

                    b.Property<string>("Parameter");

                    b.Property<string>("Query");

                    b.Property<string>("ScheduleParameter");

                    b.Property<string>("ScheduleReport");

                    b.HasKey("Id");

                    b.HasIndex("DataSourceId");

                    b.ToTable("DataReports");
                });

            modelBuilder.Entity("Infoveave.Models.DataSource", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AnalysisDataBase");

                    b.Property<string>("ColumnMappings")
                        .HasColumnType("text");

                    b.Property<string>("ConnectionString");

                    b.Property<long>("CreatedBy");

                    b.Property<DateTime>("CreatedOn");

                    b.Property<string>("Cube");

                    b.Property<bool>("IsPublic");

                    b.Property<string>("Name");

                    b.Property<string>("Server");

                    b.Property<string>("ServerType");

                    b.Property<string>("TableName");

                    b.Property<long>("TypeId");

                    b.Property<string>("ValidationSchema")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("DataSource");
                });

            modelBuilder.Entity("Infoveave.Models.Dimension", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ColumnName");

                    b.Property<long>("DataSourceId");

                    b.Property<string>("Description");

                    b.Property<int>("DrillDownLevel");

                    b.Property<bool>("IsDate");

                    b.Property<bool>("IsRangeDimension");

                    b.Property<string>("Name");

                    b.Property<string>("Query");

                    b.Property<string>("RangeDimension");

                    b.HasKey("Id");

                    b.HasIndex("DataSourceId");

                    b.ToTable("Dimension");
                });

            modelBuilder.Entity("Infoveave.Models.Formula", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("CreatedBy");

                    b.Property<DateTime>("CreatedOn");

                    b.Property<long>("DataSourceId");

                    b.Property<string>("Expression");

                    b.Property<bool>("IsPublic");

                    b.Property<string>("Name");

                    b.Property<string>("NonFormulaExpression");

                    b.HasKey("Id");

                    b.HasIndex("DataSourceId");

                    b.ToTable("Formula");
                });

            modelBuilder.Entity("Infoveave.Models.FormulaElement", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("DataSourceId");

                    b.Property<int>("ElementType");

                    b.Property<string>("Expression");

                    b.Property<string>("ExtendedExpression");

                    b.Property<long>("FormulaId");

                    b.Property<string>("LinkTo");

                    b.Property<float>("MaxRange");

                    b.Property<long?>("MeasureId");

                    b.Property<float>("MinRange");

                    b.Property<string>("Name");

                    b.Property<float>("Step");

                    b.Property<bool>("UserManaged");

                    b.Property<float>("Value");

                    b.Property<int>("ValueType");

                    b.Property<string>("key");

                    b.HasKey("Id");

                    b.ToTable("FormulaElement");
                });

            modelBuilder.Entity("Infoveave.Models.FormulaElementDimension", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("DimensionId");

                    b.Property<long>("FormulaElementsId");

                    b.Property<long>("FormulaId");

                    b.Property<long>("MeasureId");

                    b.HasKey("Id");

                    b.HasIndex("FormulaElementsId");

                    b.ToTable("FormulaElementDimension");
                });

            modelBuilder.Entity("Infoveave.Models.Infoboard", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("CreatedBy");

                    b.Property<DateTime>("CreatedOn");

                    b.Property<string>("InfoboardOptions");

                    b.Property<bool>("IsPublic");

                    b.Property<string>("Layouts")
                        .HasColumnType("text");

                    b.Property<string>("Name");

                    b.Property<string>("ShortCode");

                    b.Property<long?>("SortOrder");

                    b.HasKey("Id");

                    b.ToTable("Infoboard");
                });

            modelBuilder.Entity("Infoveave.Models.InfoboardLink", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("HorizontalSize");

                    b.Property<long>("InfoboardId");

                    b.Property<int>("PositionX");

                    b.Property<int>("PositionY");

                    b.Property<string>("SavedViewShortCode");

                    b.Property<int>("VerticalSize");

                    b.Property<long>("WidgetId");

                    b.HasKey("Id");

                    b.HasIndex("InfoboardId");

                    b.HasIndex("WidgetId");

                    b.ToTable("InfoboardLink");
                });

            modelBuilder.Entity("Infoveave.Models.Measure", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Aggregation");

                    b.Property<string>("ColumnName");

                    b.Property<DateTime>("CreatedOn");

                    b.Property<string>("DataQuery");

                    b.Property<long>("DataSourceId");

                    b.Property<string>("Description");

                    b.Property<string>("IsPercent");

                    b.Property<string>("Name");

                    b.Property<string>("Prefix");

                    b.Property<string>("Query");

                    b.Property<string>("Suffix");

                    b.HasKey("Id");

                    b.HasIndex("DataSourceId");

                    b.ToTable("Measure");
                });

            modelBuilder.Entity("Infoveave.Models.Organisation", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreatedOn");

                    b.Property<string>("Language");

                    b.Property<int>("MaximumUsers");

                    b.Property<string>("Name");

                    b.Property<int>("StarterPack");

                    b.HasKey("Id");

                    b.ToTable("Organisation");
                });

            modelBuilder.Entity("Infoveave.Models.Reports", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("CreatedBy");

                    b.Property<DateTime>("CreatedOn");

                    b.Property<string>("FileName");

                    b.Property<bool>("IsPublic");

                    b.Property<string>("MailTo");

                    b.Property<string>("Name");

                    b.Property<long>("ReportId");

                    b.Property<string>("ScheduleParameter");

                    b.Property<string>("ScheduleReport");

                    b.HasKey("Id");

                    b.ToTable("Reports");
                });

            modelBuilder.Entity("Infoveave.Models.UploadBatch", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("OrganisationDataSourceId");

                    b.Property<long>("UploadedBy");

                    b.Property<DateTime>("UploadedDate");

                    b.HasKey("Id");

                    b.ToTable("UploadBatch");
                });

            modelBuilder.Entity("Infoveave.Models.User", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ContextInfo")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedOn");

                    b.Property<string>("Email")
                        .IsRequired();

                    b.Property<string>("FirstName");

                    b.Property<string>("ImagePath");

                    b.Property<bool>("IsArchived");

                    b.Property<bool>("IsLockedOut");

                    b.Property<string>("Language");

                    b.Property<DateTime?>("LastLoginDateTime");

                    b.Property<string>("LastName");

                    b.Property<DateTime?>("LockoutDate");

                    b.Property<int>("LoginAttempts");

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PasswordSalt");

                    b.Property<string>("UserName")
                        .IsRequired();

                    b.Property<long>("UserRoleId");

                    b.HasKey("Id");

                    b.HasIndex("UserRoleId");

                    b.ToTable("User");
                });

            modelBuilder.Entity("Infoveave.Models.UserRole", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("CreatedBy");

                    b.Property<DateTime>("CreatedOn");

                    b.Property<string>("Name");

                    b.Property<string>("Permissions");

                    b.HasKey("Id");

                    b.ToTable("UserRole");
                });

            modelBuilder.Entity("Infoveave.Models.Widget", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("CreatedBy");

                    b.Property<DateTime>("CreatedOn");

                    b.Property<string>("DataSourceIds");

                    b.Property<string>("FullName");

                    b.Property<bool>("IsPublic");

                    b.Property<string>("ItemOptions");

                    b.Property<string>("ItemType");

                    b.Property<string>("Name");

                    b.Property<string>("SavedData");

                    b.Property<string>("ShortCode");

                    b.Property<long>("TimeProgression");

                    b.HasKey("Id");

                    b.ToTable("Widget");
                });

            modelBuilder.Entity("Infoveave.Models.WidgetAnnotation", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AnnotationContent")
                        .HasColumnType("text");

                    b.Property<string>("AnnotationData")
                        .HasColumnType("text");

                    b.Property<long>("CreatedBy");

                    b.Property<DateTime>("CreatedOn");

                    b.Property<string>("EndDate");

                    b.Property<string>("StartDate");

                    b.Property<long>("WidgetId");

                    b.HasKey("Id");

                    b.HasIndex("WidgetId");

                    b.ToTable("WidgetAnnotation");
                });

            modelBuilder.Entity("Infoveave.Models.AnalysisDimension", b =>
                {
                    b.HasOne("Infoveave.Models.Analysis", "Analysis")
                        .WithMany("AnalysisDimensions")
                        .HasForeignKey("AnalysisId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Infoveave.Models.AnalysisDimensionItem", b =>
                {
                    b.HasOne("Infoveave.Models.Analysis", "Analysis")
                        .WithMany("AnalysisDimensionItems")
                        .HasForeignKey("AnalysisId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Infoveave.Models.AnalysisScenario", b =>
                {
                    b.HasOne("Infoveave.Models.Analysis", "Analysis")
                        .WithMany()
                        .HasForeignKey("AnalysisId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Infoveave.Models.DataReports", b =>
                {
                    b.HasOne("Infoveave.Models.DataSource", "DataSource")
                        .WithMany()
                        .HasForeignKey("DataSourceId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Infoveave.Models.Dimension", b =>
                {
                    b.HasOne("Infoveave.Models.DataSource", "DataSource")
                        .WithMany("Dimensions")
                        .HasForeignKey("DataSourceId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Infoveave.Models.Formula", b =>
                {
                    b.HasOne("Infoveave.Models.DataSource", "DataSource")
                        .WithMany()
                        .HasForeignKey("DataSourceId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Infoveave.Models.FormulaElementDimension", b =>
                {
                    b.HasOne("Infoveave.Models.FormulaElement", "FormulaElements")
                        .WithMany("FormulaElementDimensions")
                        .HasForeignKey("FormulaElementsId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Infoveave.Models.InfoboardLink", b =>
                {
                    b.HasOne("Infoveave.Models.Infoboard", "Infoboard")
                        .WithMany("Items")
                        .HasForeignKey("InfoboardId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Infoveave.Models.Widget", "Widget")
                        .WithMany()
                        .HasForeignKey("WidgetId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Infoveave.Models.Measure", b =>
                {
                    b.HasOne("Infoveave.Models.DataSource", "DataSource")
                        .WithMany("Measures")
                        .HasForeignKey("DataSourceId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Infoveave.Models.User", b =>
                {
                    b.HasOne("Infoveave.Models.UserRole", "UserRole")
                        .WithMany("Users")
                        .HasForeignKey("UserRoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Infoveave.Models.WidgetAnnotation", b =>
                {
                    b.HasOne("Infoveave.Models.Widget", "Widget")
                        .WithMany()
                        .HasForeignKey("WidgetId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
