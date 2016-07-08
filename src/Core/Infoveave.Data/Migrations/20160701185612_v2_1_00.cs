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
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Infoveave.Data.Migrations
{
    public partial class v2_1_00 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Analysis",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreatedBy = table.Column<long>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    FormulaId = table.Column<long>(nullable: false),
                    IsPublic = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Analysis", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataSource",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AnalysisDataBase = table.Column<string>(nullable: true),
                    ColumnMappings = table.Column<string>(type: "text", nullable: true),
                    ConnectionString = table.Column<string>(nullable: true),
                    CreatedBy = table.Column<long>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Cube = table.Column<string>(nullable: true),
                    IsPublic = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Server = table.Column<string>(nullable: true),
                    ServerType = table.Column<string>(nullable: true),
                    TableName = table.Column<string>(nullable: true),
                    TypeId = table.Column<long>(nullable: false),
                    ValidationSchema = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSource", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FormulaElement",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DataSourceId = table.Column<long>(nullable: false),
                    ElementType = table.Column<int>(nullable: false),
                    Expression = table.Column<string>(nullable: true),
                    ExtendedExpression = table.Column<string>(nullable: true),
                    FormulaId = table.Column<long>(nullable: false),
                    LinkTo = table.Column<string>(nullable: true),
                    MaxRange = table.Column<float>(nullable: false),
                    MeasureId = table.Column<long>(nullable: true),
                    MinRange = table.Column<float>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Step = table.Column<float>(nullable: false),
                    UserManaged = table.Column<bool>(nullable: false),
                    Value = table.Column<float>(nullable: false),
                    ValueType = table.Column<int>(nullable: false),
                    key = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormulaElement", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Infoboard",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreatedBy = table.Column<long>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    InfoboardOptions = table.Column<string>(nullable: true),
                    IsPublic = table.Column<bool>(nullable: false),
                    Layouts = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(nullable: true),
                    ShortCode = table.Column<string>(nullable: true),
                    SortOrder = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Infoboard", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Organisation",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Language = table.Column<string>(nullable: true),
                    MaximumUsers = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    StarterPack = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organisation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreatedBy = table.Column<long>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    FileName = table.Column<string>(nullable: true),
                    IsPublic = table.Column<bool>(nullable: false),
                    MailTo = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    ReportId = table.Column<long>(nullable: false),
                    ScheduleParameter = table.Column<string>(nullable: true),
                    ScheduleReport = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UploadBatch",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    OrganisationDataSourceId = table.Column<long>(nullable: false),
                    UploadedBy = table.Column<long>(nullable: false),
                    UploadedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadBatch", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreatedBy = table.Column<long>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Permissions = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Widget",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreatedBy = table.Column<long>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    DataSourceIds = table.Column<string>(nullable: true),
                    FullName = table.Column<string>(nullable: true),
                    IsPublic = table.Column<bool>(nullable: false),
                    ItemOptions = table.Column<string>(nullable: true),
                    ItemType = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    SavedData = table.Column<string>(nullable: true),
                    ShortCode = table.Column<string>(nullable: true),
                    TimeProgression = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Widget", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AnalysisDimension",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AnalysisId = table.Column<long>(nullable: false),
                    DimensionId = table.Column<long>(nullable: false),
                    Driven = table.Column<bool>(nullable: false),
                    View = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisDimension", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnalysisDimension_Analysis_AnalysisId",
                        column: x => x.AnalysisId,
                        principalTable: "Analysis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnalysisDimensionItem",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AnalysisId = table.Column<long>(nullable: false),
                    DimensionId = table.Column<long>(nullable: false),
                    DimensionName = table.Column<string>(nullable: true),
                    MdxName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisDimensionItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnalysisDimensionItem_Analysis_AnalysisId",
                        column: x => x.AnalysisId,
                        principalTable: "Analysis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnalysisScenario",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AnalysisData = table.Column<string>(nullable: true),
                    AnalysisId = table.Column<long>(nullable: false),
                    CreatedBy = table.Column<long>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    IsPublic = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisScenario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnalysisScenario_Analysis_AnalysisId",
                        column: x => x.AnalysisId,
                        principalTable: "Analysis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DataReports",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ConnectionString = table.Column<string>(nullable: true),
                    CreatedBy = table.Column<long>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    DataSourceId = table.Column<long>(nullable: false),
                    DataStructure = table.Column<string>(nullable: true),
                    MailTo = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Parameter = table.Column<string>(nullable: true),
                    Query = table.Column<string>(nullable: true),
                    ScheduleParameter = table.Column<string>(nullable: true),
                    ScheduleReport = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataReports_DataSource_DataSourceId",
                        column: x => x.DataSourceId,
                        principalTable: "DataSource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dimension",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ColumnName = table.Column<string>(nullable: true),
                    DataSourceId = table.Column<long>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    DrillDownLevel = table.Column<int>(nullable: false),
                    IsDate = table.Column<bool>(nullable: false),
                    IsRangeDimension = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Query = table.Column<string>(nullable: true),
                    RangeDimension = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dimension", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dimension_DataSource_DataSourceId",
                        column: x => x.DataSourceId,
                        principalTable: "DataSource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Formula",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreatedBy = table.Column<long>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    DataSourceId = table.Column<long>(nullable: false),
                    Expression = table.Column<string>(nullable: true),
                    IsPublic = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    NonFormulaExpression = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Formula", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Formula_DataSource_DataSourceId",
                        column: x => x.DataSourceId,
                        principalTable: "DataSource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Measure",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Aggregation = table.Column<string>(nullable: true),
                    ColumnName = table.Column<string>(nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    DataQuery = table.Column<string>(nullable: true),
                    DataSourceId = table.Column<long>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    IsPercent = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Prefix = table.Column<string>(nullable: true),
                    Query = table.Column<string>(nullable: true),
                    Suffix = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Measure", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Measure_DataSource_DataSourceId",
                        column: x => x.DataSourceId,
                        principalTable: "DataSource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FormulaElementDimension",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DimensionId = table.Column<long>(nullable: false),
                    FormulaElementsId = table.Column<long>(nullable: false),
                    FormulaId = table.Column<long>(nullable: false),
                    MeasureId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormulaElementDimension", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormulaElementDimension_FormulaElement_FormulaElementsId",
                        column: x => x.FormulaElementsId,
                        principalTable: "FormulaElement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ContextInfo = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Email = table.Column<string>(nullable: false),
                    FirstName = table.Column<string>(nullable: true),
                    ImagePath = table.Column<string>(nullable: true),
                    IsArchived = table.Column<bool>(nullable: false),
                    IsLockedOut = table.Column<bool>(nullable: false),
                    Language = table.Column<string>(nullable: true),
                    LastLoginDateTime = table.Column<DateTime>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    LockoutDate = table.Column<DateTime>(nullable: true),
                    LoginAttempts = table.Column<int>(nullable: false),
                    PasswordHash = table.Column<string>(nullable: true),
                    PasswordSalt = table.Column<string>(nullable: true),
                    UserName = table.Column<string>(nullable: false),
                    UserRoleId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_UserRole_UserRoleId",
                        column: x => x.UserRoleId,
                        principalTable: "UserRole",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InfoboardLink",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    HorizontalSize = table.Column<int>(nullable: false),
                    InfoboardId = table.Column<long>(nullable: false),
                    PositionX = table.Column<int>(nullable: false),
                    PositionY = table.Column<int>(nullable: false),
                    SavedViewShortCode = table.Column<string>(nullable: true),
                    VerticalSize = table.Column<int>(nullable: false),
                    WidgetId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InfoboardLink", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InfoboardLink_Infoboard_InfoboardId",
                        column: x => x.InfoboardId,
                        principalTable: "Infoboard",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InfoboardLink_Widget_WidgetId",
                        column: x => x.WidgetId,
                        principalTable: "Widget",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WidgetAnnotation",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AnnotationContent = table.Column<string>(type: "text", nullable: true),
                    AnnotationData = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<long>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<string>(nullable: true),
                    StartDate = table.Column<string>(nullable: true),
                    WidgetId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WidgetAnnotation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WidgetAnnotation_Widget_WidgetId",
                        column: x => x.WidgetId,
                        principalTable: "Widget",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisDimension_AnalysisId",
                table: "AnalysisDimension",
                column: "AnalysisId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisDimensionItem_AnalysisId",
                table: "AnalysisDimensionItem",
                column: "AnalysisId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisScenario_AnalysisId",
                table: "AnalysisScenario",
                column: "AnalysisId");

            migrationBuilder.CreateIndex(
                name: "IX_DataReports_DataSourceId",
                table: "DataReports",
                column: "DataSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Dimension_DataSourceId",
                table: "Dimension",
                column: "DataSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Formula_DataSourceId",
                table: "Formula",
                column: "DataSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_FormulaElementDimension_FormulaElementsId",
                table: "FormulaElementDimension",
                column: "FormulaElementsId");

            migrationBuilder.CreateIndex(
                name: "IX_InfoboardLink_InfoboardId",
                table: "InfoboardLink",
                column: "InfoboardId");

            migrationBuilder.CreateIndex(
                name: "IX_InfoboardLink_WidgetId",
                table: "InfoboardLink",
                column: "WidgetId");

            migrationBuilder.CreateIndex(
                name: "IX_Measure_DataSourceId",
                table: "Measure",
                column: "DataSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_User_UserRoleId",
                table: "User",
                column: "UserRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_WidgetAnnotation_WidgetId",
                table: "WidgetAnnotation",
                column: "WidgetId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalysisDimension");

            migrationBuilder.DropTable(
                name: "AnalysisDimensionItem");

            migrationBuilder.DropTable(
                name: "AnalysisScenario");

            migrationBuilder.DropTable(
                name: "DataReports");

            migrationBuilder.DropTable(
                name: "Dimension");

            migrationBuilder.DropTable(
                name: "Formula");

            migrationBuilder.DropTable(
                name: "FormulaElementDimension");

            migrationBuilder.DropTable(
                name: "InfoboardLink");

            migrationBuilder.DropTable(
                name: "Measure");

            migrationBuilder.DropTable(
                name: "Organisation");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "UploadBatch");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "WidgetAnnotation");

            migrationBuilder.DropTable(
                name: "Analysis");

            migrationBuilder.DropTable(
                name: "FormulaElement");

            migrationBuilder.DropTable(
                name: "Infoboard");

            migrationBuilder.DropTable(
                name: "DataSource");

            migrationBuilder.DropTable(
                name: "UserRole");

            migrationBuilder.DropTable(
                name: "Widget");
        }
    }
}
