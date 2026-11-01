using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInboxEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InboxEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    Number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReceivedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Source = table.Column<int>(type: "int", nullable: false),
                    VesselIds = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsProcessed = table.Column<bool>(type: "bit", nullable: false),
                    IsCancelled = table.Column<bool>(type: "bit", nullable: false),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InboxEntryLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InboxEntryId = table.Column<int>(type: "int", nullable: false),
                    EntityType = table.Column<int>(type: "int", nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxEntryLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InboxEntryLinks_InboxEntries_InboxEntryId",
                        column: x => x.InboxEntryId,
                        principalTable: "InboxEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InboxEntries_Guid",
                table: "InboxEntries",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InboxEntries_IsProcessed_IsCancelled",
                table: "InboxEntries",
                columns: new[] { "IsProcessed", "IsCancelled" });

            migrationBuilder.CreateIndex(
                name: "IX_InboxEntries_Number",
                table: "InboxEntries",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InboxEntries_ReceivedDate",
                table: "InboxEntries",
                column: "ReceivedDate");

            migrationBuilder.CreateIndex(
                name: "IX_InboxEntries_Year_Sequence",
                table: "InboxEntries",
                columns: new[] { "Year", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InboxEntryLinks_EntityType_EntityId",
                table: "InboxEntryLinks",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_InboxEntryLinks_Guid",
                table: "InboxEntryLinks",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InboxEntryLinks_InboxEntryId_EntityType_EntityId",
                table: "InboxEntryLinks",
                columns: new[] { "InboxEntryId", "EntityType", "EntityId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InboxEntryLinks");

            migrationBuilder.DropTable(
                name: "InboxEntries");
        }
    }
}
