using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StoredFileFolderEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FolderId",
                table: "StoredFiles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StoredFolders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ParentFolderId = table.Column<int>(type: "int", nullable: true),
                    EntityType = table.Column<int>(type: "int", nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: true),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoredFolders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoredFolders_StoredFolders_ParentFolderId",
                        column: x => x.ParentFolderId,
                        principalTable: "StoredFolders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StoredFiles_FolderId",
                table: "StoredFiles",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_StoredFolders_EntityType_EntityId_ParentFolderId_Name",
                table: "StoredFolders",
                columns: new[] { "EntityType", "EntityId", "ParentFolderId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoredFolders_Guid",
                table: "StoredFolders",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoredFolders_ParentFolderId",
                table: "StoredFolders",
                column: "ParentFolderId");

            migrationBuilder.AddForeignKey(
                name: "FK_StoredFiles_StoredFolders_FolderId",
                table: "StoredFiles",
                column: "FolderId",
                principalTable: "StoredFolders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoredFiles_StoredFolders_FolderId",
                table: "StoredFiles");

            migrationBuilder.DropTable(
                name: "StoredFolders");

            migrationBuilder.DropIndex(
                name: "IX_StoredFiles_FolderId",
                table: "StoredFiles");

            migrationBuilder.DropColumn(
                name: "FolderId",
                table: "StoredFiles");
        }
    }
}
