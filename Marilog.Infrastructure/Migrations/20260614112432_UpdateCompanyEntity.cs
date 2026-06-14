using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCompanyEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Companies");

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CompanyBankAccounts",
                columns: table => new
                {
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IBAN = table.Column<string>(type: "nvarchar(34)", maxLength: 34, nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SwiftCode = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    AccountHolderName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false)
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
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false)
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
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Number = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false)
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

            migrationBuilder.DropColumn(
                name: "Website",
                table: "Companies");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Companies",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Companies",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
