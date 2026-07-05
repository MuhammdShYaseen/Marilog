using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UnDoPartyCompanyNav : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContractParties_Companies_CompanyId",
                table: "ContractParties");

            migrationBuilder.DropForeignKey(
                name: "FK_ContractParties_Companies_CompanyId1",
                table: "ContractParties");

            migrationBuilder.DropIndex(
                name: "IX_ContractParties_CompanyId",
                table: "ContractParties");

            migrationBuilder.DropIndex(
                name: "IX_ContractParties_CompanyId1",
                table: "ContractParties");

            migrationBuilder.DropColumn(
                name: "CompanyId1",
                table: "ContractParties");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyId1",
                table: "ContractParties",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractParties_CompanyId",
                table: "ContractParties",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractParties_CompanyId1",
                table: "ContractParties",
                column: "CompanyId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ContractParties_Companies_CompanyId",
                table: "ContractParties",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ContractParties_Companies_CompanyId1",
                table: "ContractParties",
                column: "CompanyId1",
                principalTable: "Companies",
                principalColumn: "Id");
        }
    }
}
