using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LayTimeEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DurationInMonth",
                table: "CrewContracts",
                type: "int",
                nullable: false,
                defaultValue: 6);

            migrationBuilder.CreateTable(
                name: "CharterTerms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractId = table.Column<int>(type: "int", nullable: false),
                    CargoQuantityMt = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Loading_OperationType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Loading_RateMtPerDay = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    Loading_CalendarType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Loading_NoticeHours = table.Column<int>(type: "int", nullable: true),
                    Loading_IsReversible = table.Column<bool>(type: "bit", nullable: true),
                    Discharging_OperationType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Discharging_RateMtPerDay = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    Discharging_CalendarType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Discharging_NoticeHours = table.Column<int>(type: "int", nullable: true),
                    Discharging_IsReversible = table.Column<bool>(type: "bit", nullable: true),
                    Demurrage_RateUsdPerDay = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Demurrage_OnceAlwaysOnDemurrage = table.Column<bool>(type: "bit", nullable: false),
                    Despatch_RateUsdPerDay = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Despatch_Basis = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Rules_DraftSurveyCounts = table.Column<bool>(type: "bit", nullable: false),
                    Rules_HolidaysIncluded = table.Column<bool>(type: "bit", nullable: false),
                    Rules_SundaysIncluded = table.Column<bool>(type: "bit", nullable: false),
                    Rules_TimeReversible = table.Column<bool>(type: "bit", nullable: false),
                    Rules_AllowShiftingTime = table.Column<bool>(type: "bit", nullable: false),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharterTerms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharterTerms_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LaytimeCalculations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VoyageId = table.Column<int>(type: "int", nullable: false),
                    ContractId = table.Column<int>(type: "int", nullable: false),
                    PortId = table.Column<int>(type: "int", nullable: false),
                    OperationType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CargoQuantityMt = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LaytimeCommencedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LaytimeCompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Result_AllowedDays = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Result_UsedDays = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Result_BalanceDays = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Result_DemurrageAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Result_DespatchAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Result_IsDemurrage = table.Column<bool>(type: "bit", nullable: true),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LaytimeCalculations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LaytimeExceptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LaytimeCalculationId = table.Column<int>(type: "int", nullable: false),
                    From = table.Column<DateTime>(type: "datetime2", nullable: false),
                    To = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExceptionType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Factor = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LinkedSofEventId = table.Column<int>(type: "int", nullable: true),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LaytimeExceptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LaytimeExceptions_LaytimeCalculations_LaytimeCalculationId",
                        column: x => x.LaytimeCalculationId,
                        principalTable: "LaytimeCalculations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LaytimeSegments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LaytimeCalculationId = table.Column<int>(type: "int", nullable: false),
                    From = table.Column<DateTime>(type: "datetime2", nullable: false),
                    To = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ImpactType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Factor = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LaytimeSegments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LaytimeSegments_LaytimeCalculations_LaytimeCalculationId",
                        column: x => x.LaytimeCalculationId,
                        principalTable: "LaytimeCalculations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SofEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LaytimeCalculationId = table.Column<int>(type: "int", nullable: false),
                    EventTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ImpactType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Factor = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SofEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SofEvents_LaytimeCalculations_LaytimeCalculationId",
                        column: x => x.LaytimeCalculationId,
                        principalTable: "LaytimeCalculations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CharterTerms_ContractId",
                table: "CharterTerms",
                column: "ContractId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CharterTerms_Guid",
                table: "CharterTerms",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LaytimeCalculations_ContractId",
                table: "LaytimeCalculations",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_LaytimeCalculations_Guid",
                table: "LaytimeCalculations",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LaytimeCalculations_Voyage_Operation",
                table: "LaytimeCalculations",
                columns: new[] { "VoyageId", "OperationType" });

            migrationBuilder.CreateIndex(
                name: "IX_LaytimeCalculations_VoyageId",
                table: "LaytimeCalculations",
                column: "VoyageId");

            migrationBuilder.CreateIndex(
                name: "IX_LaytimeExceptions_CalculationId",
                table: "LaytimeExceptions",
                column: "LaytimeCalculationId");

            migrationBuilder.CreateIndex(
                name: "IX_LaytimeExceptions_Guid",
                table: "LaytimeExceptions",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LaytimeSegments_CalculationId",
                table: "LaytimeSegments",
                column: "LaytimeCalculationId");

            migrationBuilder.CreateIndex(
                name: "IX_LaytimeSegments_Guid",
                table: "LaytimeSegments",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SofEvents_Calculation_Time",
                table: "SofEvents",
                columns: new[] { "LaytimeCalculationId", "EventTime" });

            migrationBuilder.CreateIndex(
                name: "IX_SofEvents_Guid",
                table: "SofEvents",
                column: "Guid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CharterTerms");

            migrationBuilder.DropTable(
                name: "LaytimeExceptions");

            migrationBuilder.DropTable(
                name: "LaytimeSegments");

            migrationBuilder.DropTable(
                name: "SofEvents");

            migrationBuilder.DropTable(
                name: "LaytimeCalculations");

            migrationBuilder.DropColumn(
                name: "DurationInMonth",
                table: "CrewContracts");
        }
    }
}
