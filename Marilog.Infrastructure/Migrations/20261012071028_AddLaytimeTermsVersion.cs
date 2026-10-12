using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLaytimeTermsVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ComputedTermsVersion",
                table: "LaytimeCalculations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TermsVersion",
                table: "CharterTerms",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ComputedTermsVersion",
                table: "LaytimeCalculations");

            migrationBuilder.DropColumn(
                name: "TermsVersion",
                table: "CharterTerms");
        }
    }
}
