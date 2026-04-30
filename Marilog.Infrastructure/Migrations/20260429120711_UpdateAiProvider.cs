using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAiProvider : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiProviders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProviderType = table.Column<int>(type: "int", nullable: false),
                    RequestTemplateJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BaseUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ModelIdentifier = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ApiVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MaxInputTokens = table.Column<int>(type: "int", nullable: false),
                    MaxOutputTokens = table.Column<int>(type: "int", nullable: false),
                    ChunkOverlapTokens = table.Column<int>(type: "int", nullable: false),
                    DefaultTemperature = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    ApiKeyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ApiKeyEncrypted = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupportsStreaming = table.Column<bool>(type: "bit", nullable: false),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiProviders", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiProviders_Guid",
                table: "AiProviders",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AiProviders_IsActive",
                table: "AiProviders",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AiProviders_ProviderType",
                table: "AiProviders",
                column: "ProviderType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiProviders");
        }
    }
}
