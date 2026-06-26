using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRelatinToVoyage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VoyageId",
                table: "Documents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthHeader",
                table: "AiProviders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_VoyageId",
                table: "Documents",
                column: "VoyageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Voyages_VoyageId",
                table: "Documents",
                column: "VoyageId",
                principalTable: "Voyages",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Voyages_VoyageId",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_VoyageId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "VoyageId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "AuthHeader",
                table: "AiProviders");
        }
    }
}
