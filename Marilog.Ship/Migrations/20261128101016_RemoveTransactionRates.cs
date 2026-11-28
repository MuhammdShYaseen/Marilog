using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Ship.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTransactionRates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExchangeRate",
                table: "OwnerRemittances");

            migrationBuilder.DropColumn(
                name: "ExchangeRate",
                table: "CrewWithdrawals");

            migrationBuilder.DropColumn(
                name: "ExchangeRate",
                table: "CrewExtras");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRate",
                table: "OwnerRemittances",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRate",
                table: "CrewWithdrawals",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRate",
                table: "CrewExtras",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
