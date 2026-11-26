using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Ship.Migrations
{
    /// <inheritdoc />
    public partial class AddShipEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CrewExtras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    CrewContractId = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    CurrencyId = table.Column<int>(type: "INTEGER", nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "TEXT", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrewExtras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrewExtras_CrewContracts_CrewContractId",
                        column: x => x.CrewContractId,
                        principalTable: "CrewContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrewExtras_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeductionTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeductionTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Mgas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Number = table.Column<int>(type: "INTEGER", nullable: false),
                    FromDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    ToDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    IsOpening = table.Column<bool>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    EndReason = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    MasterName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    PreviousBalance = table.Column<decimal>(type: "TEXT", nullable: false),
                    TotalWages = table.Column<decimal>(type: "TEXT", nullable: false),
                    TotalExpenses = table.Column<decimal>(type: "TEXT", nullable: false),
                    TotalReceived = table.Column<decimal>(type: "TEXT", nullable: false),
                    ClosingBalance = table.Column<decimal>(type: "TEXT", nullable: false),
                    ExpectedCigarettes = table.Column<decimal>(type: "TEXT", nullable: false),
                    CountedCigarettes = table.Column<decimal>(type: "TEXT", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mgas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OwnerRemittances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    CurrencyId = table.Column<int>(type: "INTEGER", nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "TEXT", nullable: false),
                    CrewContractId = table.Column<int>(type: "INTEGER", nullable: true),
                    PortId = table.Column<int>(type: "INTEGER", nullable: true),
                    Reference = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OwnerRemittances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OwnerRemittances_CrewContracts_CrewContractId",
                        column: x => x.CrewContractId,
                        principalTable: "CrewContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OwnerRemittances_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OwnerRemittances_Ports_PortId",
                        column: x => x.PortId,
                        principalTable: "Ports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShipSetup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VesselId = table.Column<int>(type: "INTEGER", nullable: false),
                    BaseCurrencyId = table.Column<int>(type: "INTEGER", nullable: false),
                    DefaultWagesPaidBy = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipSetup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShipSetup_Currencies_BaseCurrencyId",
                        column: x => x.BaseCurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShipSetup_Vessels_VesselId",
                        column: x => x.VesselId,
                        principalTable: "Vessels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CrewWithdrawals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    CrewContractId = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DeductionTypeId = table.Column<int>(type: "INTEGER", nullable: true),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    CurrencyId = table.Column<int>(type: "INTEGER", nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "TEXT", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrewWithdrawals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrewWithdrawals_CrewContracts_CrewContractId",
                        column: x => x.CrewContractId,
                        principalTable: "CrewContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrewWithdrawals_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrewWithdrawals_DeductionTypes_DeductionTypeId",
                        column: x => x.DeductionTypeId,
                        principalTable: "DeductionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MgaCashLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MgaId = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrencyId = table.Column<int>(type: "INTEGER", nullable: false),
                    Expected = table.Column<decimal>(type: "TEXT", nullable: false),
                    Counted = table.Column<decimal>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MgaCashLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MgaCashLines_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MgaCashLines_Mgas_MgaId",
                        column: x => x.MgaId,
                        principalTable: "Mgas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MgaCrewLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MgaId = table.Column<int>(type: "INTEGER", nullable: false),
                    CrewContractId = table.Column<int>(type: "INTEGER", nullable: false),
                    Days = table.Column<int>(type: "INTEGER", nullable: false),
                    PreviousBalance = table.Column<decimal>(type: "TEXT", nullable: false),
                    Wages = table.Column<decimal>(type: "TEXT", nullable: false),
                    Extras = table.Column<decimal>(type: "TEXT", nullable: false),
                    Cash = table.Column<decimal>(type: "TEXT", nullable: false),
                    Cigarettes = table.Column<decimal>(type: "TEXT", nullable: false),
                    Other = table.Column<decimal>(type: "TEXT", nullable: false),
                    PaidAshore = table.Column<decimal>(type: "TEXT", nullable: false),
                    Balance = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MgaCrewLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MgaCrewLines_CrewContracts_CrewContractId",
                        column: x => x.CrewContractId,
                        principalTable: "CrewContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MgaCrewLines_Mgas_MgaId",
                        column: x => x.MgaId,
                        principalTable: "Mgas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CrewExtras_CrewContractId_Date",
                table: "CrewExtras",
                columns: new[] { "CrewContractId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_CrewExtras_CurrencyId",
                table: "CrewExtras",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_CrewExtras_Date",
                table: "CrewExtras",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_CrewExtras_Guid",
                table: "CrewExtras",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CrewWithdrawals_CrewContractId_Date",
                table: "CrewWithdrawals",
                columns: new[] { "CrewContractId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_CrewWithdrawals_CurrencyId",
                table: "CrewWithdrawals",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_CrewWithdrawals_Date",
                table: "CrewWithdrawals",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_CrewWithdrawals_DeductionTypeId",
                table: "CrewWithdrawals",
                column: "DeductionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CrewWithdrawals_Guid",
                table: "CrewWithdrawals",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeductionTypes_Code",
                table: "DeductionTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeductionTypes_Guid",
                table: "DeductionTypes",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MgaCashLines_CurrencyId",
                table: "MgaCashLines",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_MgaCashLines_MgaId_CurrencyId",
                table: "MgaCashLines",
                columns: new[] { "MgaId", "CurrencyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MgaCrewLines_CrewContractId",
                table: "MgaCrewLines",
                column: "CrewContractId");

            migrationBuilder.CreateIndex(
                name: "IX_MgaCrewLines_MgaId_CrewContractId",
                table: "MgaCrewLines",
                columns: new[] { "MgaId", "CrewContractId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Mgas_FromDate_ToDate",
                table: "Mgas",
                columns: new[] { "FromDate", "ToDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Mgas_Guid",
                table: "Mgas",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Mgas_Number",
                table: "Mgas",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OwnerRemittances_CrewContractId",
                table: "OwnerRemittances",
                column: "CrewContractId");

            migrationBuilder.CreateIndex(
                name: "IX_OwnerRemittances_CurrencyId",
                table: "OwnerRemittances",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_OwnerRemittances_Date",
                table: "OwnerRemittances",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_OwnerRemittances_Guid",
                table: "OwnerRemittances",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OwnerRemittances_PortId",
                table: "OwnerRemittances",
                column: "PortId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipSetup_BaseCurrencyId",
                table: "ShipSetup",
                column: "BaseCurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipSetup_Guid",
                table: "ShipSetup",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShipSetup_VesselId",
                table: "ShipSetup",
                column: "VesselId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CrewExtras");

            migrationBuilder.DropTable(
                name: "CrewWithdrawals");

            migrationBuilder.DropTable(
                name: "MgaCashLines");

            migrationBuilder.DropTable(
                name: "MgaCrewLines");

            migrationBuilder.DropTable(
                name: "OwnerRemittances");

            migrationBuilder.DropTable(
                name: "ShipSetup");

            migrationBuilder.DropTable(
                name: "DeductionTypes");

            migrationBuilder.DropTable(
                name: "Mgas");
        }
    }
}
