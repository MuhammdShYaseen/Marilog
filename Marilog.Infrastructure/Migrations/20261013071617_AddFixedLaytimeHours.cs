using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFixedLaytimeHours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "LaytimeTerms_Discharging_FixedLaytimeHours",
                table: "CharterTerms",
                type: "decimal(9,2)",
                precision: 9,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LaytimeTerms_Loading_FixedLaytimeHours",
                table: "CharterTerms",
                type: "decimal(9,2)",
                precision: 9,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LaytimeTerms_Discharging_FixedLaytimeHours",
                table: "CharterTerms");

            migrationBuilder.DropColumn(
                name: "LaytimeTerms_Loading_FixedLaytimeHours",
                table: "CharterTerms");
        }
    }
}
