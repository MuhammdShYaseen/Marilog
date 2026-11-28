using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Ship.Migrations
{
    /// <inheritdoc />
    public partial class AddDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CargoType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Category = table.Column<int>(type: "INTEGER", nullable: false),
                    DefaultUnit = table.Column<int>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CargoType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StoredFolders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ParentFolderId = table.Column<int>(type: "INTEGER", nullable: true),
                    EntityType = table.Column<int>(type: "INTEGER", nullable: false),
                    EntityId = table.Column<int>(type: "INTEGER", nullable: true),
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoredFolders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoredFolders_StoredFolders_ParentFolderId",
                        column: x => x.ParentFolderId,
                        principalTable: "StoredFolders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Voyages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VesselID = table.Column<int>(type: "INTEGER", nullable: false),
                    VoyageNumber = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    VoyageMonth = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    MasterContractID = table.Column<int>(type: "INTEGER", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Voyages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Voyages_CrewContracts_MasterContractID",
                        column: x => x.MasterContractID,
                        principalTable: "CrewContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Voyages_Vessels_VesselID",
                        column: x => x.VesselID,
                        principalTable: "Vessels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StoredFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EntityType = table.Column<int>(type: "INTEGER", nullable: false),
                    EntityId = table.Column<int>(type: "INTEGER", nullable: true),
                    FolderId = table.Column<int>(type: "INTEGER", nullable: true),
                    OriginalFileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    StoredFileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    RelativePath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ThumbnailRelativePath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: true),
                    Size = table.Column<long>(type: "INTEGER", nullable: false),
                    Checksum = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoredFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoredFiles_StoredFolders_FolderId",
                        column: x => x.FolderId,
                        principalTable: "StoredFolders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BillOfLading",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VoyageID = table.Column<int>(type: "INTEGER", nullable: false),
                    BlNumber = table.Column<string>(type: "TEXT", nullable: false),
                    BlType = table.Column<int>(type: "INTEGER", nullable: false),
                    IssuanceType = table.Column<int>(type: "INTEGER", nullable: false),
                    ShipperCompanyID = table.Column<int>(type: "INTEGER", nullable: false),
                    ConsigneeCompanyID = table.Column<int>(type: "INTEGER", nullable: true),
                    ConsigneeToOrder = table.Column<string>(type: "TEXT", nullable: true),
                    NotifyPartyCompanyID = table.Column<int>(type: "INTEGER", nullable: true),
                    CarrierCompanyID = table.Column<int>(type: "INTEGER", nullable: false),
                    PortOfLoadingID = table.Column<int>(type: "INTEGER", nullable: false),
                    PortOfDischargeID = table.Column<int>(type: "INTEGER", nullable: false),
                    PlaceOfReceiptPortID = table.Column<int>(type: "INTEGER", nullable: true),
                    PlaceOfReceiptId = table.Column<int>(type: "INTEGER", nullable: true),
                    PlaceOfDeliveryPortID = table.Column<int>(type: "INTEGER", nullable: true),
                    PlaceOfDeliveryId = table.Column<int>(type: "INTEGER", nullable: true),
                    CargoDescription = table.Column<string>(type: "TEXT", nullable: false),
                    HsCode = table.Column<string>(type: "TEXT", nullable: true),
                    GrossWeightMT = table.Column<decimal>(type: "TEXT", nullable: false),
                    VolumeM3 = table.Column<decimal>(type: "TEXT", nullable: true),
                    PackageCount = table.Column<int>(type: "INTEGER", nullable: true),
                    PackageType = table.Column<string>(type: "TEXT", nullable: true),
                    MarksAndNumbers = table.Column<string>(type: "TEXT", nullable: true),
                    FreightTerms = table.Column<int>(type: "INTEGER", nullable: false),
                    FreightAmount = table.Column<string>(type: "TEXT", nullable: true),
                    Incoterms = table.Column<string>(type: "TEXT", nullable: true),
                    IssueDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    PlaceOfIssue = table.Column<string>(type: "TEXT", nullable: true),
                    OnBoardDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    OriginalCopiesCount = table.Column<int>(type: "INTEGER", nullable: false),
                    MasterBlID = table.Column<int>(type: "INTEGER", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    VoyageCargoID = table.Column<int>(type: "INTEGER", nullable: true),
                    Clauses = table.Column<string>(type: "TEXT", nullable: true),
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillOfLading", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillOfLading_BillOfLading_MasterBlID",
                        column: x => x.MasterBlID,
                        principalTable: "BillOfLading",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BillOfLading_Companies_CarrierCompanyID",
                        column: x => x.CarrierCompanyID,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BillOfLading_Companies_ConsigneeCompanyID",
                        column: x => x.ConsigneeCompanyID,
                        principalTable: "Companies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BillOfLading_Companies_NotifyPartyCompanyID",
                        column: x => x.NotifyPartyCompanyID,
                        principalTable: "Companies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BillOfLading_Companies_ShipperCompanyID",
                        column: x => x.ShipperCompanyID,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BillOfLading_Ports_PlaceOfDeliveryId",
                        column: x => x.PlaceOfDeliveryId,
                        principalTable: "Ports",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BillOfLading_Ports_PlaceOfReceiptId",
                        column: x => x.PlaceOfReceiptId,
                        principalTable: "Ports",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BillOfLading_Ports_PortOfDischargeID",
                        column: x => x.PortOfDischargeID,
                        principalTable: "Ports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BillOfLading_Ports_PortOfLoadingID",
                        column: x => x.PortOfLoadingID,
                        principalTable: "Ports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BillOfLading_Voyages_VoyageID",
                        column: x => x.VoyageID,
                        principalTable: "Voyages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DocNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DocTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Side = table.Column<int>(type: "INTEGER", nullable: false),
                    DocDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    DocDue = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    SupplierId = table.Column<int>(type: "INTEGER", nullable: true),
                    BuyerId = table.Column<int>(type: "INTEGER", nullable: true),
                    VesselId = table.Column<int>(type: "INTEGER", nullable: true),
                    PortId = table.Column<int>(type: "INTEGER", nullable: true),
                    CurrencyId = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    Reference = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    SearchVector = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    ParentDocumentId = table.Column<int>(type: "INTEGER", nullable: true),
                    VoyageId = table.Column<int>(type: "INTEGER", nullable: true),
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documents_Companies_BuyerId",
                        column: x => x.BuyerId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_Companies_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_DocumentTypes_DocTypeId",
                        column: x => x.DocTypeId,
                        principalTable: "DocumentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_Documents_ParentDocumentId",
                        column: x => x.ParentDocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_Ports_PortId",
                        column: x => x.PortId,
                        principalTable: "Ports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_Vessels_VesselId",
                        column: x => x.VesselId,
                        principalTable: "Vessels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_Voyages_VoyageId",
                        column: x => x.VoyageId,
                        principalTable: "Voyages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Color = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    StoredFileId = table.Column<int>(type: "INTEGER", nullable: false),
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tags_StoredFiles_StoredFileId",
                        column: x => x.StoredFileId,
                        principalTable: "StoredFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "document_items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DocumentId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Quantity = table.Column<decimal>(type: "TEXT", nullable: false),
                    Unit = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "TEXT", nullable: false),
                    LineTotal = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_document_items_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentAdjustments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DocumentId = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    AdjustmentDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentAdjustments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentAdjustments_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DocumentId = table.Column<int>(type: "INTEGER", nullable: false),
                    SwiftTransferId = table.Column<int>(type: "INTEGER", nullable: true),
                    Discreption = table.Column<string>(type: "TEXT", nullable: true),
                    PaidAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    PaymentDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    PaymentMethod = table.Column<int>(type: "INTEGER", nullable: false),
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payment_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BillOfLading_CarrierCompanyID",
                table: "BillOfLading",
                column: "CarrierCompanyID");

            migrationBuilder.CreateIndex(
                name: "IX_BillOfLading_ConsigneeCompanyID",
                table: "BillOfLading",
                column: "ConsigneeCompanyID");

            migrationBuilder.CreateIndex(
                name: "IX_BillOfLading_Guid",
                table: "BillOfLading",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BillOfLading_MasterBlID",
                table: "BillOfLading",
                column: "MasterBlID");

            migrationBuilder.CreateIndex(
                name: "IX_BillOfLading_NotifyPartyCompanyID",
                table: "BillOfLading",
                column: "NotifyPartyCompanyID");

            migrationBuilder.CreateIndex(
                name: "IX_BillOfLading_PlaceOfDeliveryId",
                table: "BillOfLading",
                column: "PlaceOfDeliveryId");

            migrationBuilder.CreateIndex(
                name: "IX_BillOfLading_PlaceOfReceiptId",
                table: "BillOfLading",
                column: "PlaceOfReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_BillOfLading_PortOfDischargeID",
                table: "BillOfLading",
                column: "PortOfDischargeID");

            migrationBuilder.CreateIndex(
                name: "IX_BillOfLading_PortOfLoadingID",
                table: "BillOfLading",
                column: "PortOfLoadingID");

            migrationBuilder.CreateIndex(
                name: "IX_BillOfLading_ShipperCompanyID",
                table: "BillOfLading",
                column: "ShipperCompanyID");

            migrationBuilder.CreateIndex(
                name: "IX_BillOfLading_VoyageID",
                table: "BillOfLading",
                column: "VoyageID");

            migrationBuilder.CreateIndex(
                name: "IX_CargoType_Guid",
                table: "CargoType",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_document_items_DocumentId",
                table: "document_items",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentAdjustments_DocumentId",
                table: "DocumentAdjustments",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentAdjustments_Guid",
                table: "DocumentAdjustments",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_BuyerId",
                table: "Documents",
                column: "BuyerId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_CurrencyId",
                table: "Documents",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_DocNumber",
                table: "Documents",
                column: "DocNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_DocTypeId",
                table: "Documents",
                column: "DocTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_Guid",
                table: "Documents",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_IsActive_DocDate",
                table: "Documents",
                columns: new[] { "IsActive", "DocDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ParentDocumentId",
                table: "Documents",
                column: "ParentDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_PortId",
                table: "Documents",
                column: "PortId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_SearchVector",
                table: "Documents",
                column: "SearchVector");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_SupplierId",
                table: "Documents",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_VesselId",
                table: "Documents",
                column: "VesselId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_VoyageId",
                table: "Documents",
                column: "VoyageId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_Code",
                table: "DocumentTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_Guid",
                table: "DocumentTypes",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payment_DocumentId",
                table: "Payment",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_Guid",
                table: "Payment",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoredFiles_Checksum",
                table: "StoredFiles",
                column: "Checksum");

            migrationBuilder.CreateIndex(
                name: "IX_StoredFiles_EntityType_EntityId",
                table: "StoredFiles",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_StoredFiles_FolderId",
                table: "StoredFiles",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_StoredFiles_Guid",
                table: "StoredFiles",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoredFolders_EntityType_EntityId_ParentFolderId_Name",
                table: "StoredFolders",
                columns: new[] { "EntityType", "EntityId", "ParentFolderId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoredFolders_Guid",
                table: "StoredFolders",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoredFolders_ParentFolderId",
                table: "StoredFolders",
                column: "ParentFolderId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Guid",
                table: "Tags",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_StoredFileId_Name",
                table: "Tags",
                columns: new[] { "StoredFileId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Voyages_Guid",
                table: "Voyages",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Voyages_MasterContractID",
                table: "Voyages",
                column: "MasterContractID");

            migrationBuilder.CreateIndex(
                name: "IX_Voyages_Status",
                table: "Voyages",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Voyages_VesselID",
                table: "Voyages",
                column: "VesselID");

            migrationBuilder.CreateIndex(
                name: "IX_Voyages_VoyageMonth",
                table: "Voyages",
                column: "VoyageMonth");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BillOfLading");

            migrationBuilder.DropTable(
                name: "CargoType");

            migrationBuilder.DropTable(
                name: "document_items");

            migrationBuilder.DropTable(
                name: "DocumentAdjustments");

            migrationBuilder.DropTable(
                name: "Payment");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "StoredFiles");

            migrationBuilder.DropTable(
                name: "DocumentTypes");

            migrationBuilder.DropTable(
                name: "Voyages");

            migrationBuilder.DropTable(
                name: "StoredFolders");
        }
    }
}
