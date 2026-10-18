using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CrewPayroll_Refactored : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CrewPayrollDisbursements_Offices_OfficeId",
                table: "CrewPayrollDisbursements");

            migrationBuilder.DropForeignKey(
                name: "FK_CrewPayrollDisbursements_Voyages_VoyageId",
                table: "CrewPayrollDisbursements");

            migrationBuilder.DropForeignKey(
                name: "FK_CrewPayrollDisbursements_swift_transfers_SwiftTransferId",
                table: "CrewPayrollDisbursements");

            migrationBuilder.DropIndex(
                name: "IX_CrewPayrollDisbursements_Method",
                table: "CrewPayrollDisbursements");

            migrationBuilder.DropIndex(
                name: "IX_CrewPayrollDisbursements_OfficeId",
                table: "CrewPayrollDisbursements");

            migrationBuilder.DropIndex(
                name: "IX_CrewPayrollDisbursements_SwiftTransferId",
                table: "CrewPayrollDisbursements");

            migrationBuilder.DropIndex(
                name: "IX_CrewPayrollDisbursements_VoyageId",
                table: "CrewPayrollDisbursements");

            migrationBuilder.DropColumn(
                name: "OfficeId",
                table: "CrewPayrollDisbursements");

            migrationBuilder.DropColumn(
                name: "SwiftTransferId",
                table: "CrewPayrollDisbursements");

            migrationBuilder.DropColumn(
                name: "VoyageId",
                table: "CrewPayrollDisbursements");

            migrationBuilder.AlterColumn<decimal>(
                name: "GrossAmount",
                table: "CrewPayrolls",
                type: "decimal(12,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(12,2)",
                oldComment: "USD — BasicWage + Allowances - Deductions");

            migrationBuilder.AlterColumn<decimal>(
                name: "Deductions",
                table: "CrewPayrolls",
                type: "decimal(12,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(12,2)",
                oldDefaultValue: 0m,
                oldComment: "USD");

            migrationBuilder.AlterColumn<decimal>(
                name: "BasicWage",
                table: "CrewPayrolls",
                type: "decimal(12,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(12,2)",
                oldComment: "USD");

            migrationBuilder.AlterColumn<decimal>(
                name: "Allowances",
                table: "CrewPayrolls",
                type: "decimal(12,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(12,2)",
                oldDefaultValue: 0m,
                oldComment: "USD");

            migrationBuilder.AddColumn<string>(
                name: "CurrencyCode",
                table: "CrewPayrolls",
                type: "nchar(3)",
                fixedLength: true,
                maxLength: 3,
                nullable: false,
                defaultValue: "USD");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "CrewPayrollDisbursements",
                type: "decimal(12,2)",
                nullable: false,
                comment: "In the payroll's currency",
                oldClrType: typeof(decimal),
                oldType: "decimal(12,2)",
                oldComment: "USD");

            migrationBuilder.AddColumn<string>(
                name: "Reference",
                table: "CrewPayrollDisbursements",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrencyCode",
                table: "CrewPayrolls");

            migrationBuilder.DropColumn(
                name: "Reference",
                table: "CrewPayrollDisbursements");

            migrationBuilder.AlterColumn<decimal>(
                name: "GrossAmount",
                table: "CrewPayrolls",
                type: "decimal(12,2)",
                nullable: false,
                comment: "USD — BasicWage + Allowances - Deductions",
                oldClrType: typeof(decimal),
                oldType: "decimal(12,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Deductions",
                table: "CrewPayrolls",
                type: "decimal(12,2)",
                nullable: false,
                defaultValue: 0m,
                comment: "USD",
                oldClrType: typeof(decimal),
                oldType: "decimal(12,2)",
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "BasicWage",
                table: "CrewPayrolls",
                type: "decimal(12,2)",
                nullable: false,
                comment: "USD",
                oldClrType: typeof(decimal),
                oldType: "decimal(12,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Allowances",
                table: "CrewPayrolls",
                type: "decimal(12,2)",
                nullable: false,
                defaultValue: 0m,
                comment: "USD",
                oldClrType: typeof(decimal),
                oldType: "decimal(12,2)",
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "CrewPayrollDisbursements",
                type: "decimal(12,2)",
                nullable: false,
                comment: "USD",
                oldClrType: typeof(decimal),
                oldType: "decimal(12,2)",
                oldComment: "In the payroll's currency");

            migrationBuilder.AddColumn<int>(
                name: "OfficeId",
                table: "CrewPayrollDisbursements",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SwiftTransferId",
                table: "CrewPayrollDisbursements",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VoyageId",
                table: "CrewPayrollDisbursements",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CrewPayrollDisbursements_Method",
                table: "CrewPayrollDisbursements",
                column: "Method");

            migrationBuilder.CreateIndex(
                name: "IX_CrewPayrollDisbursements_OfficeId",
                table: "CrewPayrollDisbursements",
                column: "OfficeId",
                filter: "[OfficeId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CrewPayrollDisbursements_SwiftTransferId",
                table: "CrewPayrollDisbursements",
                column: "SwiftTransferId",
                filter: "[SwiftTransferId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CrewPayrollDisbursements_VoyageId",
                table: "CrewPayrollDisbursements",
                column: "VoyageId",
                filter: "[VoyageId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_CrewPayrollDisbursements_Offices_OfficeId",
                table: "CrewPayrollDisbursements",
                column: "OfficeId",
                principalTable: "Offices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CrewPayrollDisbursements_Voyages_VoyageId",
                table: "CrewPayrollDisbursements",
                column: "VoyageId",
                principalTable: "Voyages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CrewPayrollDisbursements_swift_transfers_SwiftTransferId",
                table: "CrewPayrollDisbursements",
                column: "SwiftTransferId",
                principalTable: "swift_transfers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
