using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUnlessSoonerCommenced : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "LaytimeTerms_Discharging_UnlessSoonerCommenced",
                table: "CharterTerms",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LaytimeTerms_Loading_UnlessSoonerCommenced",
                table: "CharterTerms",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LaytimeTerms_Discharging_UnlessSoonerCommenced",
                table: "CharterTerms");

            migrationBuilder.DropColumn(
                name: "LaytimeTerms_Loading_UnlessSoonerCommenced",
                table: "CharterTerms");
        }
    }
}
