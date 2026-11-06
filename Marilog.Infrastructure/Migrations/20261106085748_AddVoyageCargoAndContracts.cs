using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVoyageCargoAndContracts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VoyageCargo",
                columns: table => new
                {
                    VoyageCargoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StopID = table.Column<int>(type: "int", nullable: false),
                    CargoTypeID = table.Column<int>(type: "int", nullable: false),
                    Operation = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(14,3)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    WeightMt = table.Column<decimal>(type: "decimal(14,3)", nullable: true),
                    ContractID = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
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
                        name: "FK_VoyageCargo_Contracts_ContractID",
                        column: x => x.ContractID,
                        principalTable: "Contracts",
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
                name: "VoyageContracts",
                columns: table => new
                {
                    ContractID = table.Column<int>(type: "int", nullable: false),
                    VoyageID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoyageContracts", x => new { x.VoyageID, x.ContractID });
                    table.ForeignKey(
                        name: "FK_VoyageContracts_Contracts_ContractID",
                        column: x => x.ContractID,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VoyageContracts_Voyages_VoyageID",
                        column: x => x.VoyageID,
                        principalTable: "Voyages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VoyageCargoBulk",
                columns: table => new
                {
                    VoyageCargoID = table.Column<int>(type: "int", nullable: false),
                    DraftSurveyMt = table.Column<decimal>(type: "decimal(14,3)", nullable: true),
                    ShoreFigureMt = table.Column<decimal>(type: "decimal(14,3)", nullable: true)
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
                    VoyageCargoID = table.Column<int>(type: "int", nullable: false),
                    PackageCount = table.Column<int>(type: "int", nullable: true),
                    PackageType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    VolumeCbm = table.Column<decimal>(type: "decimal(14,3)", nullable: true)
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
                name: "IX_VoyageCargo_CargoTypeID",
                table: "VoyageCargo",
                column: "CargoTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_VoyageCargo_ContractID",
                table: "VoyageCargo",
                column: "ContractID");

            migrationBuilder.CreateIndex(
                name: "IX_VoyageCargo_StopID",
                table: "VoyageCargo",
                column: "StopID");

            migrationBuilder.CreateIndex(
                name: "IX_VoyageContracts_ContractID",
                table: "VoyageContracts",
                column: "ContractID");
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
        }
    }
}
