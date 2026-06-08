using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PaymentToEntityType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "payments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "Guid",
                table: "payments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "payments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "payments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "payments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_payments_Guid",
                table: "payments",
                column: "Guid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_payments_Guid",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "Guid",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "payments");
        }
    }
}
