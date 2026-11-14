using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RestructureAiProvider : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AiProviders_IsActive",
                table: "AiProviders");

            migrationBuilder.DropIndex(
                name: "IX_AiProviders_ProviderType",
                table: "AiProviders");

            migrationBuilder.DropColumn(
                name: "ApiVersion",
                table: "AiProviders");

            migrationBuilder.DropColumn(
                name: "AuthHeader",
                table: "AiProviders");

            migrationBuilder.DropColumn(
                name: "SupportsStreaming",
                table: "AiProviders");

            migrationBuilder.RenameColumn(
                name: "ApiKeyName",
                table: "AiProviders",
                newName: "ResponseTextPath");

            migrationBuilder.AlterColumn<string>(
                name: "ApiKeyEncrypted",
                table: "AiProviders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "ApiKeyHint",
                table: "AiProviders",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthHeaderName",
                table: "AiProviders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AuthPrefix",
                table: "AiProviders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExtraHeadersJson",
                table: "AiProviders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Purpose",
                table: "AiProviders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AiProviders_Purpose_IsActive",
                table: "AiProviders",
                columns: new[] { "Purpose", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AiProviders_Purpose_IsActive",
                table: "AiProviders");

            migrationBuilder.DropColumn(
                name: "ApiKeyHint",
                table: "AiProviders");

            migrationBuilder.DropColumn(
                name: "AuthHeaderName",
                table: "AiProviders");

            migrationBuilder.DropColumn(
                name: "AuthPrefix",
                table: "AiProviders");

            migrationBuilder.DropColumn(
                name: "ExtraHeadersJson",
                table: "AiProviders");

            migrationBuilder.DropColumn(
                name: "Purpose",
                table: "AiProviders");

            migrationBuilder.RenameColumn(
                name: "ResponseTextPath",
                table: "AiProviders",
                newName: "ApiKeyName");

            migrationBuilder.AlterColumn<string>(
                name: "ApiKeyEncrypted",
                table: "AiProviders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApiVersion",
                table: "AiProviders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthHeader",
                table: "AiProviders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "SupportsStreaming",
                table: "AiProviders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_AiProviders_IsActive",
                table: "AiProviders",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AiProviders_ProviderType",
                table: "AiProviders",
                column: "ProviderType");
        }
    }
}
