using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Ship.Migrations
{
    /// <inheritdoc />
    public partial class SetupBackend : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShipSetup_Currencies_BaseCurrencyId",
                table: "ShipSetup");

            migrationBuilder.DropIndex(
                name: "IX_ShipSetup_BaseCurrencyId",
                table: "ShipSetup");

            migrationBuilder.DropColumn(
                name: "BaseCurrencyId",
                table: "ShipSetup");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Mgas",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CrewPayrolls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ContractId = table.Column<int>(type: "INTEGER", nullable: false),
                    PayrollMonth = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    WorkingDays = table.Column<int>(type: "INTEGER", nullable: false),
                    BasicWage = table.Column<decimal>(type: "TEXT", nullable: false),
                    Allowances = table.Column<decimal>(type: "TEXT", nullable: false),
                    Deductions = table.Column<decimal>(type: "TEXT", nullable: false),
                    GrossAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    CurrencyId = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CancelReason = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrewPayrolls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrewPayrolls_CrewContracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "CrewContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrewPayrolls_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CrewPayrollDisbursements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PayrollId = table.Column<int>(type: "INTEGER", nullable: false),
                    Method = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    PaidOn = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    RecipientName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    RecipientIdNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Reference = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    CancelReason = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrewPayrollDisbursements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrewPayrollDisbursements_CrewPayrolls_PayrollId",
                        column: x => x.PayrollId,
                        principalTable: "CrewPayrolls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CrewPayrollDisbursements_Guid",
                table: "CrewPayrollDisbursements",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CrewPayrollDisbursements_PayrollId",
                table: "CrewPayrollDisbursements",
                column: "PayrollId");

            migrationBuilder.CreateIndex(
                name: "IX_CrewPayrolls_ContractId_PayrollMonth",
                table: "CrewPayrolls",
                columns: new[] { "ContractId", "PayrollMonth" },
                unique: true,
                filter: "\"Status\" <> 'Cancelled'");

            migrationBuilder.CreateIndex(
                name: "IX_CrewPayrolls_CurrencyId",
                table: "CrewPayrolls",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_CrewPayrolls_Guid",
                table: "CrewPayrolls",
                column: "Guid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CrewPayrollDisbursements");

            migrationBuilder.DropTable(
                name: "CrewPayrolls");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Mgas");

            migrationBuilder.AddColumn<int>(
                name: "BaseCurrencyId",
                table: "ShipSetup",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ShipSetup_BaseCurrencyId",
                table: "ShipSetup",
                column: "BaseCurrencyId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShipSetup_Currencies_BaseCurrencyId",
                table: "ShipSetup",
                column: "BaseCurrencyId",
                principalTable: "Currencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
