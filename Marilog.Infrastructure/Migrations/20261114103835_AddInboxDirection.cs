using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInboxDirection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InboxEntries_Year_Sequence",
                table: "InboxEntries");

            migrationBuilder.AddColumn<int>(
                name: "Direction",
                table: "InboxEntries",
                type: "int",
                nullable: false,
                defaultValue: 1);   // existing entries are all incoming (InboxDirection.In));

            migrationBuilder.CreateIndex(
                name: "IX_InboxEntries_Direction_Year_Sequence",
                table: "InboxEntries",
                columns: new[] { "Direction", "Year", "Sequence" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InboxEntries_Direction_Year_Sequence",
                table: "InboxEntries");

            migrationBuilder.DropColumn(
                name: "Direction",
                table: "InboxEntries");

            migrationBuilder.CreateIndex(
                name: "IX_InboxEntries_Year_Sequence",
                table: "InboxEntries",
                columns: new[] { "Year", "Sequence" },
                unique: true);
        }
    }
}
