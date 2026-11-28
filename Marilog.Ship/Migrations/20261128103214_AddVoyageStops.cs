using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Ship.Migrations
{
    /// <inheritdoc />
    public partial class AddVoyageStops : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CargoTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DefaultUnit = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CargoTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VoyageContracts",
                columns: table => new
                {
                    ContractID = table.Column<int>(type: "INTEGER", nullable: false),
                    VoyageID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoyageContracts", x => new { x.VoyageID, x.ContractID });
                    table.ForeignKey(
                        name: "FK_VoyageContracts_Voyages_VoyageID",
                        column: x => x.VoyageID,
                        principalTable: "Voyages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VoyageStops",
                columns: table => new
                {
                    StopID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VoyageID = table.Column<int>(type: "INTEGER", nullable: false),
                    PortID = table.Column<int>(type: "INTEGER", nullable: false),
                    StopOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    ArrivalDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DepartureDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PurposeOfCall = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoyageStops", x => x.StopID);
                    table.ForeignKey(
                        name: "FK_VoyageStops_Ports_PortID",
                        column: x => x.PortID,
                        principalTable: "Ports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VoyageStops_Voyages_VoyageID",
                        column: x => x.VoyageID,
                        principalTable: "Voyages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VoyageCargo",
                columns: table => new
                {
                    VoyageCargoID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StopID = table.Column<int>(type: "INTEGER", nullable: false),
                    CargoTypeID = table.Column<int>(type: "INTEGER", nullable: false),
                    Operation = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Quantity = table.Column<decimal>(type: "TEXT", nullable: false),
                    Unit = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    WeightMt = table.Column<decimal>(type: "TEXT", nullable: true),
                    ContractID = table.Column<int>(type: "INTEGER", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoyageCargo", x => x.VoyageCargoID);
                    table.ForeignKey(
                        name: "FK_VoyageCargo_CargoTypes_CargoTypeID",
                        column: x => x.CargoTypeID,
                        principalTable: "CargoTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VoyageCargo_VoyageStops_StopID",
                        column: x => x.StopID,
                        principalTable: "VoyageStops",
                        principalColumn: "StopID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VoyageCargoBulk",
                columns: table => new
                {
                    VoyageCargoID = table.Column<int>(type: "INTEGER", nullable: false),
                    DraftSurveyMt = table.Column<decimal>(type: "TEXT", nullable: true),
                    ShoreFigureMt = table.Column<decimal>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoyageCargoBulk", x => x.VoyageCargoID);
                    table.ForeignKey(
                        name: "FK_VoyageCargoBulk_VoyageCargo_VoyageCargoID",
                        column: x => x.VoyageCargoID,
                        principalTable: "VoyageCargo",
                        principalColumn: "VoyageCargoID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VoyageCargoGeneral",
                columns: table => new
                {
                    VoyageCargoID = table.Column<int>(type: "INTEGER", nullable: false),
                    PackageCount = table.Column<int>(type: "INTEGER", nullable: true),
                    PackageType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    VolumeCbm = table.Column<decimal>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoyageCargoGeneral", x => x.VoyageCargoID);
                    table.ForeignKey(
                        name: "FK_VoyageCargoGeneral_VoyageCargo_VoyageCargoID",
                        column: x => x.VoyageCargoID,
                        principalTable: "VoyageCargo",
                        principalColumn: "VoyageCargoID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CargoTypes_Code",
                table: "CargoTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CargoTypes_Guid",
                table: "CargoTypes",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VoyageCargo_CargoTypeID",
                table: "VoyageCargo",
                column: "CargoTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_VoyageCargo_StopID",
                table: "VoyageCargo",
                column: "StopID");

            migrationBuilder.CreateIndex(
                name: "IX_VoyageStops_PortID",
                table: "VoyageStops",
                column: "PortID");

            migrationBuilder.CreateIndex(
                name: "IX_VoyageStops_VoyageID_StopOrder",
                table: "VoyageStops",
                columns: new[] { "VoyageID", "StopOrder" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VoyageCargoBulk");

            migrationBuilder.DropTable(
                name: "VoyageCargoGeneral");

            migrationBuilder.DropTable(
                name: "VoyageContracts");

            migrationBuilder.DropTable(
                name: "VoyageCargo");

            migrationBuilder.DropTable(
                name: "CargoTypes");

            migrationBuilder.DropTable(
                name: "VoyageStops");
        }
    }
}
