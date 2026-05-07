using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateAppThemeEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppThemes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ThemeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ThemeKey = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    PrimaryColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    SecondaryColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    AppBarColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    BackgroundColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    SurfaceColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    ErrorColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    SuccessColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    WarningColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    FontFamily = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BaseFontSize = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IsDarkMode = table.Column<bool>(type: "bit", nullable: false),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppThemes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppThemes_Guid",
                table: "AppThemes",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppThemes_ThemeKey",
                table: "AppThemes",
                column: "ThemeKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppThemes");
        }
    }
}
