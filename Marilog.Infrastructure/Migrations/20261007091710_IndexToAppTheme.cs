using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IndexToAppTheme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AppThemes_IsDefault",
                table: "AppThemes",
                column: "IsDefault",
                unique: true,
                filter: "[IsDefault] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppThemes_IsDefault",
                table: "AppThemes");
        }
    }
}
