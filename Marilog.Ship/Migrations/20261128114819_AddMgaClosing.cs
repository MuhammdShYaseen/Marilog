using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Ship.Migrations
{
    /// <inheritdoc />
    public partial class AddMgaClosing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Mgas",
                type: "TEXT",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CountDifference",
                table: "Mgas",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeDifference",
                table: "Mgas",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OfficeDeductions",
                table: "Mgas",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyWage",
                table: "MgaCrewLines",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "WagesPaidBy",
                table: "MgaCrewLines",
                type: "TEXT",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RateToBase",
                table: "MgaCashLines",
                type: "TEXT",
                nullable: true);

            migrationBuilder.Sql("UPDATE CrewWithdrawals SET Date = Date || ' 00:00:00' WHERE length(Date) = 10;");
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "Mgas");

            migrationBuilder.DropColumn(
                name: "CountDifference",
                table: "Mgas");

            migrationBuilder.DropColumn(
                name: "ExchangeDifference",
                table: "Mgas");

            migrationBuilder.DropColumn(
                name: "OfficeDeductions",
                table: "Mgas");

            migrationBuilder.DropColumn(
                name: "MonthlyWage",
                table: "MgaCrewLines");

            migrationBuilder.DropColumn(
                name: "WagesPaidBy",
                table: "MgaCrewLines");

            migrationBuilder.DropColumn(
                name: "RateToBase",
                table: "MgaCashLines");
        }
    }
}
