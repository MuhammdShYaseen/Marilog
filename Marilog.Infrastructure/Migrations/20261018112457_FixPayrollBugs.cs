using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixPayrollBugs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CrewPayrolls_ContractId_PayrollMonth",
                table: "CrewPayrolls");

            migrationBuilder.CreateIndex(
                name: "IX_CrewPayrolls_ContractId_PayrollMonth",
                table: "CrewPayrolls",
                columns: new[] { "ContractId", "PayrollMonth" },
                unique: true,
                filter: "[Status] <> 'Cancelled'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CrewPayrolls_ContractId_PayrollMonth",
                table: "CrewPayrolls");

            migrationBuilder.CreateIndex(
                name: "IX_CrewPayrolls_ContractId_PayrollMonth",
                table: "CrewPayrolls",
                columns: new[] { "ContractId", "PayrollMonth" },
                unique: true);
        }
    }
}
