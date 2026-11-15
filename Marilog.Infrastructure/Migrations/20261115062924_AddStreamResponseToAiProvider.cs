using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStreamResponseToAiProvider : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StreamTextPath",
                table: "AiProviders",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StreamTextPath",
                table: "AiProviders");
        }
    }
}
