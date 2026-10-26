using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSwiftBankFee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChargeBearer",
                table: "swift_transfers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "InstructedAmount",
                table: "swift_transfers",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "swift_bank_fees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SwiftTransferId = table.Column<int>(type: "int", nullable: false),
                    Stage = table.Column<int>(type: "int", nullable: false),
                    BankId = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsDeducted = table.Column<bool>(type: "bit", nullable: false),
                    FeeDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_swift_bank_fees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_swift_bank_fees_Banks_BankId",
                        column: x => x.BankId,
                        principalTable: "Banks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_swift_bank_fees_swift_transfers_SwiftTransferId",
                        column: x => x.SwiftTransferId,
                        principalTable: "swift_transfers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_swift_bank_fees_BankId",
                table: "swift_bank_fees",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_swift_bank_fees_Guid",
                table: "swift_bank_fees",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_swift_bank_fees_SwiftTransferId",
                table: "swift_bank_fees",
                column: "SwiftTransferId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "swift_bank_fees");

            migrationBuilder.DropColumn(
                name: "ChargeBearer",
                table: "swift_transfers");

            migrationBuilder.DropColumn(
                name: "InstructedAmount",
                table: "swift_transfers");
        }
    }
}
