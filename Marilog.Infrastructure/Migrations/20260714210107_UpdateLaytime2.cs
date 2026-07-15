using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLaytime2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Rules_SundaysIncluded",
                table: "CharterTerms",
                newName: "Rules_WeekendIncluded");

            migrationBuilder.AddColumn<string>(
                name: "Rules_WeekendDay1",
                table: "CharterTerms",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Rules_WeekendDay2",
                table: "CharterTerms",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rules_WeekendDay1",
                table: "CharterTerms");

            migrationBuilder.DropColumn(
                name: "Rules_WeekendDay2",
                table: "CharterTerms");

            migrationBuilder.RenameColumn(
                name: "Rules_WeekendIncluded",
                table: "CharterTerms",
                newName: "Rules_SundaysIncluded");
        }
    }
}
