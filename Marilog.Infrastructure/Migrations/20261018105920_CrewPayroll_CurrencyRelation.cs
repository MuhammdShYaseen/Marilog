using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CrewPayroll_CurrencyRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrencyCode",
                table: "CrewPayrolls");

            migrationBuilder.AddColumn<int>(
                name: "CurrencyId",
                table: "CrewPayrolls",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_CrewPayrolls_CurrencyId",
                table: "CrewPayrolls",
                column: "CurrencyId");

            migrationBuilder.AddForeignKey(
                name: "FK_CrewPayrolls_Currencies_CurrencyId",
                table: "CrewPayrolls",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CrewPayrolls_Currencies_CurrencyId",
                table: "CrewPayrolls");

            migrationBuilder.DropIndex(
                name: "IX_CrewPayrolls_CurrencyId",
                table: "CrewPayrolls");

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                table: "CrewPayrolls");

            migrationBuilder.AddColumn<string>(
                name: "CurrencyCode",
                table: "CrewPayrolls",
                type: "nchar(3)",
                fixedLength: true,
                maxLength: 3,
                nullable: false,
                defaultValue: "USD");
        }
    }
}
