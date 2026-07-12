using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class swiftRelationWithBank : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceiverBank",
                table: "swift_transfers");

            migrationBuilder.DropColumn(
                name: "SenderBank",
                table: "swift_transfers");

            migrationBuilder.AddColumn<int>(
                name: "ReceiverBankId",
                table: "swift_transfers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SenderBankId",
                table: "swift_transfers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_swift_transfers_ReceiverBankId",
                table: "swift_transfers",
                column: "ReceiverBankId");

            migrationBuilder.CreateIndex(
                name: "IX_swift_transfers_SenderBankId",
                table: "swift_transfers",
                column: "SenderBankId");

            migrationBuilder.AddForeignKey(
                name: "FK_swift_transfers_Banks_ReceiverBankId",
                table: "swift_transfers",
                column: "ReceiverBankId",
                principalTable: "Banks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_swift_transfers_Banks_SenderBankId",
                table: "swift_transfers",
                column: "SenderBankId",
                principalTable: "Banks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_swift_transfers_Banks_ReceiverBankId",
                table: "swift_transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_swift_transfers_Banks_SenderBankId",
                table: "swift_transfers");

            migrationBuilder.DropIndex(
                name: "IX_swift_transfers_ReceiverBankId",
                table: "swift_transfers");

            migrationBuilder.DropIndex(
                name: "IX_swift_transfers_SenderBankId",
                table: "swift_transfers");

            migrationBuilder.DropColumn(
                name: "ReceiverBankId",
                table: "swift_transfers");

            migrationBuilder.DropColumn(
                name: "SenderBankId",
                table: "swift_transfers");

            migrationBuilder.AddColumn<string>(
                name: "ReceiverBank",
                table: "swift_transfers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SenderBank",
                table: "swift_transfers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}
