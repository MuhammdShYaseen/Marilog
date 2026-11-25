using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCrewContractWagesPaidBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WagesPaidBy",
                table: "CrewContracts",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Office");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WagesPaidBy",
                table: "CrewContracts");
        }
    }
}
