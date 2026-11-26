using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Ship.Migrations
{
    /// <inheritdoc />
    public partial class InitialShip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CountryCode = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    CountryName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CurrencyCode = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    CurrencyName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Symbol = table.Column<string>(type: "TEXT", maxLength: 5, nullable: true),
                    ExchangeRate = table.Column<decimal>(type: "TEXT", nullable: false),
                    IsBaseCurrency = table.Column<bool>(type: "INTEGER", nullable: false),
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ranks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RankCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    RankName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Department = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ranks", x => x.Id);
                    table.CheckConstraint("CK_Ranks_Department", "\"Department\" IN ('DECK', 'ENGINE', 'CATERING')");
                });

            migrationBuilder.CreateTable(
                name: "VesselTypes",
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
                    table.PrimaryKey("PK_VesselTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CompanyName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CountryId = table.Column<int>(type: "INTEGER", nullable: true),
                    ContactName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    Address = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    Website = table.Column<string>(type: "TEXT", nullable: true),
                    RegistrationNumber = table.Column<string>(type: "TEXT", nullable: true),
                    IsOwnCompany = table.Column<bool>(type: "INTEGER", nullable: false),
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Companies_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Persons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Nationality = table.Column<int>(type: "INTEGER", nullable: true),
                    PassportNo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    PassportExpiry = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    SeamanBookNo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    BankName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    IBAN = table.Column<string>(type: "TEXT", maxLength: 34, nullable: true),
                    BankSwiftCode = table.Column<string>(type: "TEXT", maxLength: 11, nullable: true),
                    SuitabilityRating = table.Column<int>(type: "INTEGER", nullable: false),
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Persons_Countries_Nationality",
                        column: x => x.Nationality,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Ports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PortCode = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    PortName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    CountryID = table.Column<int>(type: "INTEGER", nullable: true),
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ports_Countries_CountryID",
                        column: x => x.CountryID,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompanyBankAccounts",
                columns: table => new
                {
                    CompanyId = table.Column<int>(type: "INTEGER", nullable: false),
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    IBAN = table.Column<string>(type: "TEXT", maxLength: 34, nullable: false),
                    BankName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SwiftCode = table.Column<string>(type: "TEXT", maxLength: 11, nullable: true),
                    CurrencyId = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountHolderName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    IsPrimary = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyBankAccounts", x => new { x.CompanyId, x.Id });
                    table.ForeignKey(
                        name: "FK_CompanyBankAccounts_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompanyBankAccounts_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompanyEmails",
                columns: table => new
                {
                    CompanyId = table.Column<int>(type: "INTEGER", nullable: false),
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Label = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    IsPrimary = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyEmails", x => new { x.CompanyId, x.Id });
                    table.ForeignKey(
                        name: "FK_CompanyEmails_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanyPhones",
                columns: table => new
                {
                    CompanyId = table.Column<int>(type: "INTEGER", nullable: false),
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Number = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Label = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    IsPrimary = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyPhones", x => new { x.CompanyId, x.Id });
                    table.ForeignKey(
                        name: "FK_CompanyPhones_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vessels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CompanyID = table.Column<int>(type: "INTEGER", nullable: false),
                    VesselName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    IMONumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    GrossTonnage = table.Column<decimal>(type: "TEXT", nullable: true),
                    FlagCountryID = table.Column<int>(type: "INTEGER", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    VesselTypeId = table.Column<int>(type: "INTEGER", nullable: true),
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vessels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vessels_Companies_CompanyID",
                        column: x => x.CompanyID,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vessels_Countries_FlagCountryID",
                        column: x => x.FlagCountryID,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vessels_VesselTypes_VesselTypeId",
                        column: x => x.VesselTypeId,
                        principalTable: "VesselTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PersonCertificates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Certificate_CertificateName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Certificate_CertificateNumber = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Certificate_IssuingAuthority = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Certificate_IssueDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    Certificate_ExpiryDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    Certificate_Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    PersonId = table.Column<int>(type: "INTEGER", nullable: true),
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonCertificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonCertificates_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonSeaServices",
                columns: table => new
                {
                    PersonId = table.Column<int>(type: "INTEGER", nullable: false),
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    RankId = table.Column<int>(type: "INTEGER", nullable: false),
                    ExperienceInMonths = table.Column<int>(type: "INTEGER", nullable: false),
                    VesselSizeInMT = table.Column<decimal>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonSeaServices", x => new { x.PersonId, x.Id });
                    table.ForeignKey(
                        name: "FK_PersonSeaServices_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CrewContracts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PersonID = table.Column<int>(type: "INTEGER", nullable: false),
                    VesselID = table.Column<int>(type: "INTEGER", nullable: false),
                    RankID = table.Column<int>(type: "INTEGER", nullable: false),
                    MonthlyWage = table.Column<decimal>(type: "TEXT", nullable: false),
                    SignOnDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    SignOffDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    SignOnPort = table.Column<int>(type: "INTEGER", nullable: true),
                    SignOffPort = table.Column<int>(type: "INTEGER", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DurationInMonth = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 6),
                    WagesPaidBy = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrewContracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrewContracts_Persons_PersonID",
                        column: x => x.PersonID,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrewContracts_Ports_SignOffPort",
                        column: x => x.SignOffPort,
                        principalTable: "Ports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrewContracts_Ports_SignOnPort",
                        column: x => x.SignOnPort,
                        principalTable: "Ports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrewContracts_Ranks_RankID",
                        column: x => x.RankID,
                        principalTable: "Ranks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrewContracts_Vessels_VesselID",
                        column: x => x.VesselID,
                        principalTable: "Vessels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VesselCertificates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Certificate_CertificateName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Certificate_CertificateNumber = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Certificate_IssuingAuthority = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Certificate_IssueDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    Certificate_ExpiryDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    Certificate_Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    VesselId = table.Column<int>(type: "INTEGER", nullable: true),
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VesselCertificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VesselCertificates_Vessels_VesselId",
                        column: x => x.VesselId,
                        principalTable: "Vessels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "IX_CompanyBankAccounts_CompanyId_IBAN",
                table: "CompanyBankAccounts",
                columns: new[] { "CompanyId", "IBAN" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyBankAccounts_CurrencyId",
                table: "CompanyBankAccounts",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyEmails_CompanyId_Address",
                table: "CompanyEmails",
                columns: new[] { "CompanyId", "Address" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyPhones_CompanyId_Number_Type",
                table: "CompanyPhones",
                columns: new[] { "CompanyId", "Number", "Type" },
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
                filter: "\"IsBaseCurrency\" = 1");

            migrationBuilder.CreateIndex(
                name: "IX_PersonCertificates_Guid",
                table: "PersonCertificates",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonCertificates_PersonId",
                table: "PersonCertificates",
                column: "PersonId");

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
                name: "IX_VesselCertificates_Guid",
                table: "VesselCertificates",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VesselCertificates_VesselId",
                table: "VesselCertificates",
                column: "VesselId");

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
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vessels_VesselTypeId",
                table: "Vessels",
                column: "VesselTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_VesselTypes_Code",
                table: "VesselTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VesselTypes_Guid",
                table: "VesselTypes",
                column: "Guid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanyBankAccounts");

            migrationBuilder.DropTable(
                name: "CompanyEmails");

            migrationBuilder.DropTable(
                name: "CompanyPhones");

            migrationBuilder.DropTable(
                name: "CrewContracts");

            migrationBuilder.DropTable(
                name: "PersonCertificates");

            migrationBuilder.DropTable(
                name: "PersonSeaServices");

            migrationBuilder.DropTable(
                name: "VesselCertificates");

            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropTable(
                name: "Ports");

            migrationBuilder.DropTable(
                name: "Ranks");

            migrationBuilder.DropTable(
                name: "Persons");

            migrationBuilder.DropTable(
                name: "Vessels");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "VesselTypes");

            migrationBuilder.DropTable(
                name: "Countries");
        }
    }
}
