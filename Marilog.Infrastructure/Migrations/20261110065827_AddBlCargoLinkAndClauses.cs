using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBlCargoLinkAndClauses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BillsOfLading_BlNumber",
                table: "BillsOfLading");

            migrationBuilder.AddColumn<string>(
                name: "Clauses",
                table: "BillsOfLading",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VoyageCargoID",
                table: "BillsOfLading",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BillsOfLading_VoyageCargoID",
                table: "BillsOfLading",
                column: "VoyageCargoID");

            migrationBuilder.AddForeignKey(
                name: "FK_BillsOfLading_VoyageCargo_VoyageCargoID",
                table: "BillsOfLading",
                column: "VoyageCargoID",
                principalTable: "VoyageCargo",
                principalColumn: "VoyageCargoID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BillsOfLading_VoyageCargo_VoyageCargoID",
                table: "BillsOfLading");

            migrationBuilder.DropIndex(
                name: "IX_BillsOfLading_VoyageCargoID",
                table: "BillsOfLading");

            migrationBuilder.DropColumn(
                name: "Clauses",
                table: "BillsOfLading");

            migrationBuilder.DropColumn(
                name: "VoyageCargoID",
                table: "BillsOfLading");

            migrationBuilder.CreateIndex(
                name: "IX_BillsOfLading_BlNumber",
                table: "BillsOfLading",
                column: "BlNumber",
                unique: true);
        }
    }
}
