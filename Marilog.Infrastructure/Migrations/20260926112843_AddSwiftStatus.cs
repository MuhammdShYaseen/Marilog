using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSwiftStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "swift_transfers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ReceivedDate",
                table: "swift_transfers",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "swift_transfers",
                type: "int",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.CreateIndex(
                name: "IX_swift_transfers_Status",
                table: "swift_transfers",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_swift_transfers_Status",
                table: "swift_transfers");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "swift_transfers");

            migrationBuilder.DropColumn(
                name: "ReceivedDate",
                table: "swift_transfers");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "swift_transfers");
        }
    }
}
