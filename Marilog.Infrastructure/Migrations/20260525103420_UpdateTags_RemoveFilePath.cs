using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTags_RemoveFilePath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Documents_DocumentId",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "Documents");

            migrationBuilder.RenameColumn(
                name: "DocumentId",
                table: "Tags",
                newName: "StoredFileId");

            migrationBuilder.RenameIndex(
                name: "IX_Tags_DocumentId_Name",
                table: "Tags",
                newName: "IX_Tags_StoredFileId_Name");

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_StoredFiles_StoredFileId",
                table: "Tags",
                column: "StoredFileId",
                principalTable: "StoredFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tags_StoredFiles_StoredFileId",
                table: "Tags");

            migrationBuilder.RenameColumn(
                name: "StoredFileId",
                table: "Tags",
                newName: "DocumentId");

            migrationBuilder.RenameIndex(
                name: "IX_Tags_StoredFileId_Name",
                table: "Tags",
                newName: "IX_Tags_DocumentId_Name");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "Documents",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Documents_DocumentId",
                table: "Tags",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
