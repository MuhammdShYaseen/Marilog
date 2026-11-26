using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Ship.Migrations
{
    /// <inheritdoc />
    public partial class AddUnlock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShipKeyEncrypted",
                table: "ShipSetup",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UnlockLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Target = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    TargetId = table.Column<int>(type: "INTEGER", nullable: true),
                    RequestCode = table.Column<string>(type: "TEXT", maxLength: 6, nullable: false),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    UnlockedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DataBefore = table.Column<string>(type: "TEXT", nullable: true),
                    DataAfter = table.Column<string>(type: "TEXT", nullable: true),
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnlockLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UnlockLogs_Guid",
                table: "UnlockLogs",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnlockLogs_Target_TargetId",
                table: "UnlockLogs",
                columns: new[] { "Target", "TargetId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UnlockLogs");

            migrationBuilder.DropColumn(
                name: "ShipKeyEncrypted",
                table: "ShipSetup");
        }
    }
}
