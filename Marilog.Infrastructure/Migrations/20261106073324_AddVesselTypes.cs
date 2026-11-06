using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVesselTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VesselTypeId",
                table: "Vessels",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "VesselTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VesselTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vessels_VesselTypeId",
                table: "Vessels",
                column: "VesselTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_VesselTypes_Code",
                table: "VesselTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VesselTypes_Guid",
                table: "VesselTypes",
                column: "Guid",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Vessels_VesselTypes_VesselTypeId",
                table: "Vessels",
                column: "VesselTypeId",
                principalTable: "VesselTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vessels_VesselTypes_VesselTypeId",
                table: "Vessels");

            migrationBuilder.DropTable(
                name: "VesselTypes");

            migrationBuilder.DropIndex(
                name: "IX_Vessels_VesselTypeId",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "VesselTypeId",
                table: "Vessels");
        }
    }
}
