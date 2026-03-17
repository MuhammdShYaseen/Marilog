using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Marilog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    CountryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CountryCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    CountryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.CountryID);
                });

            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    CurrencyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    ExchangeRate = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    IsBaseCurrency = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Emails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Direction = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Draft"),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExternalId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Emails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ranks",
                columns: table => new
                {
                    RankID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RankCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RankName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Department = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ranks", x => x.RankID);
                    table.CheckConstraint("CK_Ranks_Department", "Department IN ('DECK', 'ENGINE', 'CATERING')");
                });

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    CompanyID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CountryId = table.Column<int>(type: "int", nullable: true),
                    ContactName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Id = table.Column<int>(type: "int", nullable: false),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.CompanyID);
                    table.ForeignKey(
                        name: "FK_Companies_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "CountryID");
                });

            migrationBuilder.CreateTable(
                name: "Offices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OfficeName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CountryId = table.Column<int>(type: "int", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ContactName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Offices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Offices_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "CountryID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Persons",
                columns: table => new
                {
                    PersonID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Nationality = table.Column<int>(type: "int", nullable: true),
                    PassportNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PassportExpiry = table.Column<DateOnly>(type: "date", nullable: true),
                    SeamanBookNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IBAN = table.Column<string>(type: "nvarchar(34)", maxLength: 34, nullable: true),
                    BankSwiftCode = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    Id = table.Column<int>(type: "int", nullable: false),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persons", x => x.PersonID);
                    table.ForeignKey(
                        name: "FK_Persons_Countries_Nationality",
                        column: x => x.Nationality,
                        principalTable: "Countries",
                        principalColumn: "CountryID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Ports",
                columns: table => new
                {
                    PortID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PortCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PortName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CountryID = table.Column<int>(type: "int", nullable: true),
                    Id = table.Column<int>(type: "int", nullable: false),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ports", x => x.PortID);
                    table.ForeignKey(
                        name: "FK_Ports_Countries_CountryID",
                        column: x => x.CountryID,
                        principalTable: "Countries",
                        principalColumn: "CountryID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EmailAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmailId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailAttachments_Emails_EmailId",
                        column: x => x.EmailId,
                        principalTable: "Emails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmailParticipants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmailId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ParticipantType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ParticipantId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EmailAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailParticipants_Emails_EmailId",
                        column: x => x.EmailId,
                        principalTable: "Emails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "swift_transfers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SwiftReference = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TransactionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SenderCompanyId = table.Column<int>(type: "int", nullable: true),
                    ReceiverCompanyId = table.Column<int>(type: "int", nullable: true),
                    SenderBank = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ReceiverBank = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PaymentReference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RawMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_swift_transfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_swift_transfers_Companies_ReceiverCompanyId",
                        column: x => x.ReceiverCompanyId,
                        principalTable: "Companies",
                        principalColumn: "CompanyID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_swift_transfers_Companies_SenderCompanyId",
                        column: x => x.SenderCompanyId,
                        principalTable: "Companies",
                        principalColumn: "CompanyID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_swift_transfers_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Vessels",
                columns: table => new
                {
                    VesselID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyID = table.Column<int>(type: "int", nullable: false),
                    VesselName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    IMONumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    GrossTonnage = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    FlagCountryID = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Id = table.Column<int>(type: "int", nullable: false),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vessels", x => x.VesselID);
                    table.ForeignKey(
                        name: "FK_Vessels_Companies_CompanyID",
                        column: x => x.CompanyID,
                        principalTable: "Companies",
                        principalColumn: "CompanyID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vessels_Countries_FlagCountryID",
                        column: x => x.FlagCountryID,
                        principalTable: "Countries",
                        principalColumn: "CountryID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CrewContracts",
                columns: table => new
                {
                    ContractID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonID = table.Column<int>(type: "int", nullable: false),
                    VesselID = table.Column<int>(type: "int", nullable: false),
                    RankID = table.Column<int>(type: "int", nullable: false),
                    MonthlyWage = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    SignOnDate = table.Column<DateOnly>(type: "date", nullable: false),
                    SignOffDate = table.Column<DateOnly>(type: "date", nullable: true),
                    SignOnPort = table.Column<int>(type: "int", nullable: true),
                    SignOffPort = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Id = table.Column<int>(type: "int", nullable: false),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrewContracts", x => x.ContractID);
                    table.ForeignKey(
                        name: "FK_CrewContracts_Persons_PersonID",
                        column: x => x.PersonID,
                        principalTable: "Persons",
                        principalColumn: "PersonID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrewContracts_Ports_SignOffPort",
                        column: x => x.SignOffPort,
                        principalTable: "Ports",
                        principalColumn: "PortID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CrewContracts_Ports_SignOnPort",
                        column: x => x.SignOnPort,
                        principalTable: "Ports",
                        principalColumn: "PortID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CrewContracts_Ranks_RankID",
                        column: x => x.RankID,
                        principalTable: "Ranks",
                        principalColumn: "RankID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrewContracts_Vessels_VesselID",
                        column: x => x.VesselID,
                        principalTable: "Vessels",
                        principalColumn: "VesselID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DocTypeId = table.Column<int>(type: "int", nullable: false),
                    DocDate = table.Column<DateOnly>(type: "date", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: true),
                    BuyerId = table.Column<int>(type: "int", nullable: true),
                    VesselId = table.Column<int>(type: "int", nullable: true),
                    PortId = table.Column<int>(type: "int", nullable: true),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ParentDocumentId = table.Column<int>(type: "int", nullable: true),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documents_Companies_BuyerId",
                        column: x => x.BuyerId,
                        principalTable: "Companies",
                        principalColumn: "CompanyID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Documents_Companies_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Companies",
                        principalColumn: "CompanyID",
                        onDelete: ReferentialAction.SetNull);
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
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Documents_Ports_PortId",
                        column: x => x.PortId,
                        principalTable: "Ports",
                        principalColumn: "PortID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Documents_Vessels_VesselId",
                        column: x => x.VesselId,
                        principalTable: "Vessels",
                        principalColumn: "VesselID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CrewPayrolls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractId = table.Column<int>(type: "int", nullable: false),
                    PayrollMonth = table.Column<DateOnly>(type: "date", nullable: false),
                    WorkingDays = table.Column<int>(type: "int", nullable: false),
                    BasicWage = table.Column<decimal>(type: "decimal(12,2)", nullable: false, comment: "USD"),
                    Allowances = table.Column<decimal>(type: "decimal(12,2)", nullable: false, defaultValue: 0m, comment: "USD"),
                    Deductions = table.Column<decimal>(type: "decimal(12,2)", nullable: false, defaultValue: 0m, comment: "USD"),
                    GrossAmount = table.Column<decimal>(type: "decimal(12,2)", nullable: false, comment: "USD — BasicWage + Allowances - Deductions"),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Draft"),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrewPayrolls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrewPayrolls_CrewContracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "CrewContracts",
                        principalColumn: "ContractID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Voyages",
                columns: table => new
                {
                    VoyageID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VesselID = table.Column<int>(type: "int", nullable: false),
                    VoyageNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    VoyageMonth = table.Column<DateOnly>(type: "date", nullable: false),
                    MasterContractID = table.Column<int>(type: "int", nullable: true),
                    DeparturePortID = table.Column<int>(type: "int", nullable: true),
                    ArrivalPortID = table.Column<int>(type: "int", nullable: true),
                    DepartureDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ArrivalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CargoType = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CargoQuantityMT = table.Column<decimal>(type: "decimal(14,3)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "PLANNED"),
                    PreviousMasterBalance = table.Column<decimal>(type: "decimal(12,2)", nullable: false, defaultValue: 0m),
                    CashOnBoard = table.Column<decimal>(type: "decimal(12,2)", nullable: false, defaultValue: 0m),
                    CigarettesOnBoard = table.Column<decimal>(type: "decimal(12,2)", nullable: false, defaultValue: 0m),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Id = table.Column<int>(type: "int", nullable: false),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Voyages", x => x.VoyageID);
                    table.CheckConstraint("CK_Voyages_Status", "Status IN ('PLANNED','UNDERWAY','COMPLETED','CANCELLED')");
                    table.ForeignKey(
                        name: "FK_Voyages_CrewContracts_MasterContractID",
                        column: x => x.MasterContractID,
                        principalTable: "CrewContracts",
                        principalColumn: "ContractID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Voyages_Ports_ArrivalPortID",
                        column: x => x.ArrivalPortID,
                        principalTable: "Ports",
                        principalColumn: "PortID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Voyages_Ports_DeparturePortID",
                        column: x => x.DeparturePortID,
                        principalTable: "Ports",
                        principalColumn: "PortID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Voyages_Vessels_VesselID",
                        column: x => x.VesselID,
                        principalTable: "Vessels",
                        principalColumn: "VesselID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "document_items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentId = table.Column<int>(type: "int", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(14,4)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(14,4)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
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
                name: "payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentId = table.Column<int>(type: "int", nullable: false),
                    SwiftTransferId = table.Column<int>(type: "int", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payments_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_payments_swift_transfers_SwiftTransferId",
                        column: x => x.SwiftTransferId,
                        principalTable: "swift_transfers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CrewPayrollDisbursements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PayrollId = table.Column<int>(type: "int", nullable: false),
                    Method = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, comment: "CashOnBoard | CashAtOffice | BankTransfer"),
                    Amount = table.Column<decimal>(type: "decimal(12,2)", nullable: false, comment: "USD"),
                    PaidOn = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    VoyageId = table.Column<int>(type: "int", nullable: true),
                    OfficeId = table.Column<int>(type: "int", nullable: true),
                    RecipientName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RecipientIdNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SwiftTransferId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CancelReason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrewPayrollDisbursements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrewPayrollDisbursements_CrewPayrolls_PayrollId",
                        column: x => x.PayrollId,
                        principalTable: "CrewPayrolls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CrewPayrollDisbursements_Offices_OfficeId",
                        column: x => x.OfficeId,
                        principalTable: "Offices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrewPayrollDisbursements_Voyages_VoyageId",
                        column: x => x.VoyageId,
                        principalTable: "Voyages",
                        principalColumn: "VoyageID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrewPayrollDisbursements_swift_transfers_SwiftTransferId",
                        column: x => x.SwiftTransferId,
                        principalTable: "swift_transfers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VoyageStops",
                columns: table => new
                {
                    StopID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VoyageID = table.Column<int>(type: "int", nullable: false),
                    PortID = table.Column<int>(type: "int", nullable: false),
                    StopOrder = table.Column<int>(type: "int", nullable: false),
                    ArrivalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DepartureDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PurposeOfCall = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoyageStops", x => x.StopID);
                    table.ForeignKey(
                        name: "FK_VoyageStops_Ports_PortID",
                        column: x => x.PortID,
                        principalTable: "Ports",
                        principalColumn: "PortID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_VoyageStops_Voyages_VoyageID",
                        column: x => x.VoyageID,
                        principalTable: "Voyages",
                        principalColumn: "VoyageID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "DocumentTypes",
                columns: new[] { "Id", "Code", "CreatedAt", "Guid", "IsActive", "IsDeleted", "Name", "SortOrder", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "QUOTATION", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("11111111-0000-0000-0000-000000000001"), true, false, "Sales Quotation", 1, null },
                    { 2, "DELIVERY_NOTE", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("11111111-0000-0000-0000-000000000002"), true, false, "Delivery Note", 2, null },
                    { 3, "TAX_INVOICE", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("11111111-0000-0000-0000-000000000003"), true, false, "Tax Invoice", 3, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Companies_CountryId",
                table: "Companies",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Guid",
                table: "Companies",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Countries_CountryCode",
                table: "Countries",
                column: "CountryCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Countries_Guid",
                table: "Countries",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CrewContracts_Guid",
                table: "CrewContracts",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CrewContracts_IsActive_SignOffDate",
                table: "CrewContracts",
                columns: new[] { "IsActive", "SignOffDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CrewContracts_PersonID",
                table: "CrewContracts",
                column: "PersonID");

            migrationBuilder.CreateIndex(
                name: "IX_CrewContracts_RankID",
                table: "CrewContracts",
                column: "RankID");

            migrationBuilder.CreateIndex(
                name: "IX_CrewContracts_SignOffPort",
                table: "CrewContracts",
                column: "SignOffPort");

            migrationBuilder.CreateIndex(
                name: "IX_CrewContracts_SignOnPort",
                table: "CrewContracts",
                column: "SignOnPort");

            migrationBuilder.CreateIndex(
                name: "IX_CrewContracts_VesselID",
                table: "CrewContracts",
                column: "VesselID");

            migrationBuilder.CreateIndex(
                name: "IX_CrewPayrollDisbursements_Guid",
                table: "CrewPayrollDisbursements",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CrewPayrollDisbursements_Method",
                table: "CrewPayrollDisbursements",
                column: "Method");

            migrationBuilder.CreateIndex(
                name: "IX_CrewPayrollDisbursements_OfficeId",
                table: "CrewPayrollDisbursements",
                column: "OfficeId",
                filter: "[OfficeId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CrewPayrollDisbursements_PayrollId",
                table: "CrewPayrollDisbursements",
                column: "PayrollId");

            migrationBuilder.CreateIndex(
                name: "IX_CrewPayrollDisbursements_Status",
                table: "CrewPayrollDisbursements",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CrewPayrollDisbursements_SwiftTransferId",
                table: "CrewPayrollDisbursements",
                column: "SwiftTransferId",
                filter: "[SwiftTransferId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CrewPayrollDisbursements_VoyageId",
                table: "CrewPayrollDisbursements",
                column: "VoyageId",
                filter: "[VoyageId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CrewPayrolls_ContractId_PayrollMonth",
                table: "CrewPayrolls",
                columns: new[] { "ContractId", "PayrollMonth" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CrewPayrolls_Guid",
                table: "CrewPayrolls",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CrewPayrolls_PayrollMonth",
                table: "CrewPayrolls",
                column: "PayrollMonth");

            migrationBuilder.CreateIndex(
                name: "IX_CrewPayrolls_Status",
                table: "CrewPayrolls",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_CurrencyCode",
                table: "Currencies",
                column: "CurrencyCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_Guid",
                table: "Currencies",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_IsBaseCurrency",
                table: "Currencies",
                column: "IsBaseCurrency",
                unique: true,
                filter: "[IsBaseCurrency] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_document_items_DocumentId",
                table: "document_items",
                column: "DocumentId");

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
                name: "IX_Documents_ParentDocumentId",
                table: "Documents",
                column: "ParentDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_PortId",
                table: "Documents",
                column: "PortId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_SupplierId",
                table: "Documents",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_VesselId",
                table: "Documents",
                column: "VesselId");

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
                name: "IX_EmailAttachments_EmailId",
                table: "EmailAttachments",
                column: "EmailId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailAttachments_Guid",
                table: "EmailAttachments",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailParticipants_EmailId",
                table: "EmailParticipants",
                column: "EmailId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailParticipants_ParticipantType_ParticipantId",
                table: "EmailParticipants",
                columns: new[] { "ParticipantType", "ParticipantId" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailParticipants_Role",
                table: "EmailParticipants",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_Emails_EntityType_EntityId",
                table: "Emails",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_Emails_ExternalId",
                table: "Emails",
                column: "ExternalId");

            migrationBuilder.CreateIndex(
                name: "IX_Emails_Guid",
                table: "Emails",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Emails_SentAt",
                table: "Emails",
                column: "SentAt");

            migrationBuilder.CreateIndex(
                name: "IX_Emails_Status",
                table: "Emails",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Offices_CountryId",
                table: "Offices",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Offices_Guid",
                table: "Offices",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payments_DocumentId",
                table: "payments",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_payments_SwiftTransferId",
                table: "payments",
                column: "SwiftTransferId");

            migrationBuilder.CreateIndex(
                name: "IX_Persons_Guid",
                table: "Persons",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Persons_Nationality",
                table: "Persons",
                column: "Nationality");

            migrationBuilder.CreateIndex(
                name: "IX_Ports_CountryID",
                table: "Ports",
                column: "CountryID");

            migrationBuilder.CreateIndex(
                name: "IX_Ports_Guid",
                table: "Ports",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ports_PortCode",
                table: "Ports",
                column: "PortCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ranks_Guid",
                table: "Ranks",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ranks_RankCode",
                table: "Ranks",
                column: "RankCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_swift_transfers_CurrencyId",
                table: "swift_transfers",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_swift_transfers_Guid",
                table: "swift_transfers",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_swift_transfers_ReceiverCompanyId",
                table: "swift_transfers",
                column: "ReceiverCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_swift_transfers_SenderCompanyId",
                table: "swift_transfers",
                column: "SenderCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_swift_transfers_SwiftReference",
                table: "swift_transfers",
                column: "SwiftReference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_swift_transfers_TransactionDate",
                table: "swift_transfers",
                column: "TransactionDate");

            migrationBuilder.CreateIndex(
                name: "IX_Vessels_CompanyID",
                table: "Vessels",
                column: "CompanyID");

            migrationBuilder.CreateIndex(
                name: "IX_Vessels_FlagCountryID",
                table: "Vessels",
                column: "FlagCountryID");

            migrationBuilder.CreateIndex(
                name: "IX_Vessels_Guid",
                table: "Vessels",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vessels_IMONumber",
                table: "Vessels",
                column: "IMONumber",
                unique: true,
                filter: "[IMONumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Voyages_ArrivalPortID",
                table: "Voyages",
                column: "ArrivalPortID");

            migrationBuilder.CreateIndex(
                name: "IX_Voyages_DeparturePortID",
                table: "Voyages",
                column: "DeparturePortID");

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
                name: "CrewPayrollDisbursements");

            migrationBuilder.DropTable(
                name: "document_items");

            migrationBuilder.DropTable(
                name: "EmailAttachments");

            migrationBuilder.DropTable(
                name: "EmailParticipants");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "VoyageStops");

            migrationBuilder.DropTable(
                name: "CrewPayrolls");

            migrationBuilder.DropTable(
                name: "Offices");

            migrationBuilder.DropTable(
                name: "Emails");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "swift_transfers");

            migrationBuilder.DropTable(
                name: "Voyages");

            migrationBuilder.DropTable(
                name: "DocumentTypes");

            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropTable(
                name: "CrewContracts");

            migrationBuilder.DropTable(
                name: "Persons");

            migrationBuilder.DropTable(
                name: "Ports");

            migrationBuilder.DropTable(
                name: "Ranks");

            migrationBuilder.DropTable(
                name: "Vessels");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "Countries");
        }
    }
}
