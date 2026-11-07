using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLegacyVoyageFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Voyages_Ports_ArrivalPortID",
                table: "Voyages");

            migrationBuilder.DropForeignKey(
                name: "FK_Voyages_Ports_DeparturePortID",
                table: "Voyages");

            migrationBuilder.DropIndex(
                name: "IX_Voyages_ArrivalPortID",
                table: "Voyages");

            migrationBuilder.DropIndex(
                name: "IX_Voyages_DeparturePortID",
                table: "Voyages");

            migrationBuilder.DropColumn(
                name: "ArrivalDate",
                table: "Voyages");

            migrationBuilder.DropColumn(
                name: "ArrivalPortID",
                table: "Voyages");

            migrationBuilder.DropColumn(
                name: "CargoQuantityMT",
                table: "Voyages");

            migrationBuilder.DropColumn(
                name: "CargoType",
                table: "Voyages");

            migrationBuilder.DropColumn(
                name: "CashOnBoard",
                table: "Voyages");

            migrationBuilder.DropColumn(
                name: "CigarettesOnBoard",
                table: "Voyages");

            migrationBuilder.DropColumn(
                name: "DepartureDate",
                table: "Voyages");

            migrationBuilder.DropColumn(
                name: "DeparturePortID",
                table: "Voyages");

            migrationBuilder.DropColumn(
                name: "PreviousMasterBalance",
                table: "Voyages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ArrivalDate",
                table: "Voyages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ArrivalPortID",
                table: "Voyages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CargoQuantityMT",
                table: "Voyages",
                type: "decimal(14,3)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CargoType",
                table: "Voyages",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CashOnBoard",
                table: "Voyages",
                type: "decimal(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CigarettesOnBoard",
                table: "Voyages",
                type: "decimal(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "DepartureDate",
                table: "Voyages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeparturePortID",
                table: "Voyages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PreviousMasterBalance",
                table: "Voyages",
                type: "decimal(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Voyages_ArrivalPortID",
                table: "Voyages",
                column: "ArrivalPortID");

            migrationBuilder.CreateIndex(
                name: "IX_Voyages_DeparturePortID",
                table: "Voyages",
                column: "DeparturePortID");

            migrationBuilder.AddForeignKey(
                name: "FK_Voyages_Ports_ArrivalPortID",
                table: "Voyages",
                column: "ArrivalPortID",
                principalTable: "Ports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Voyages_Ports_DeparturePortID",
                table: "Voyages",
                column: "DeparturePortID",
                principalTable: "Ports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}