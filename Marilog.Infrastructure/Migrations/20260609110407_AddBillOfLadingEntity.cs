using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBillOfLadingEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BillsOfLading",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VoyageID = table.Column<int>(type: "int", nullable: false),
                    BlNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BlType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IssuanceType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ShipperCompanyID = table.Column<int>(type: "int", nullable: false),
                    ConsigneeCompanyID = table.Column<int>(type: "int", nullable: true),
                    ConsigneeToOrder = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NotifyPartyCompanyID = table.Column<int>(type: "int", nullable: true),
                    CarrierCompanyID = table.Column<int>(type: "int", nullable: false),
                    PortOfLoadingID = table.Column<int>(type: "int", nullable: false),
                    PortOfDischargeID = table.Column<int>(type: "int", nullable: false),
                    PlaceOfReceiptPortID = table.Column<int>(type: "int", nullable: true),
                    PlaceOfDeliveryPortID = table.Column<int>(type: "int", nullable: true),
                    CargoDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    HsCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    GrossWeightMT = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    VolumeM3 = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    PackageCount = table.Column<int>(type: "int", nullable: true),
                    PackageType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MarksAndNumbers = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FreightTerms = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FreightAmount = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Incoterms = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IssueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PlaceOfIssue = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OnBoardDate = table.Column<DateOnly>(type: "date", nullable: true),
                    OriginalCopiesCount = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                    MasterBlID = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillsOfLading", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillsOfLading_BillsOfLading_MasterBlID",
                        column: x => x.MasterBlID,
                        principalTable: "BillsOfLading",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BillsOfLading_Companies_CarrierCompanyID",
                        column: x => x.CarrierCompanyID,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BillsOfLading_Companies_ConsigneeCompanyID",
                        column: x => x.ConsigneeCompanyID,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BillsOfLading_Companies_NotifyPartyCompanyID",
                        column: x => x.NotifyPartyCompanyID,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BillsOfLading_Companies_ShipperCompanyID",
                        column: x => x.ShipperCompanyID,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BillsOfLading_Ports_PlaceOfDeliveryPortID",
                        column: x => x.PlaceOfDeliveryPortID,
                        principalTable: "Ports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BillsOfLading_Ports_PlaceOfReceiptPortID",
                        column: x => x.PlaceOfReceiptPortID,
                        principalTable: "Ports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BillsOfLading_Ports_PortOfDischargeID",
                        column: x => x.PortOfDischargeID,
                        principalTable: "Ports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BillsOfLading_Ports_PortOfLoadingID",
                        column: x => x.PortOfLoadingID,
                        principalTable: "Ports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BillsOfLading_Voyages_VoyageID",
                        column: x => x.VoyageID,
                        principalTable: "Voyages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BillsOfLading_BlNumber",
                table: "BillsOfLading",
                column: "BlNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BillsOfLading_CarrierCompanyID",
                table: "BillsOfLading",
                column: "CarrierCompanyID");

            migrationBuilder.CreateIndex(
                name: "IX_BillsOfLading_ConsigneeCompanyID",
                table: "BillsOfLading",
                column: "ConsigneeCompanyID");

            migrationBuilder.CreateIndex(
                name: "IX_BillsOfLading_Guid",
                table: "BillsOfLading",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BillsOfLading_MasterBlID",
                table: "BillsOfLading",
                column: "MasterBlID");

            migrationBuilder.CreateIndex(
                name: "IX_BillsOfLading_NotifyPartyCompanyID",
                table: "BillsOfLading",
                column: "NotifyPartyCompanyID");

            migrationBuilder.CreateIndex(
                name: "IX_BillsOfLading_PlaceOfDeliveryPortID",
                table: "BillsOfLading",
                column: "PlaceOfDeliveryPortID");

            migrationBuilder.CreateIndex(
                name: "IX_BillsOfLading_PlaceOfReceiptPortID",
                table: "BillsOfLading",
                column: "PlaceOfReceiptPortID");

            migrationBuilder.CreateIndex(
                name: "IX_BillsOfLading_PortOfDischargeID",
                table: "BillsOfLading",
                column: "PortOfDischargeID");

            migrationBuilder.CreateIndex(
                name: "IX_BillsOfLading_PortOfLoadingID",
                table: "BillsOfLading",
                column: "PortOfLoadingID");

            migrationBuilder.CreateIndex(
                name: "IX_BillsOfLading_ShipperCompanyID",
                table: "BillsOfLading",
                column: "ShipperCompanyID");

            migrationBuilder.CreateIndex(
                name: "IX_BillsOfLading_VoyageID_BlNumber",
                table: "BillsOfLading",
                columns: new[] { "VoyageID", "BlNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BillsOfLading");
        }
    }
}
