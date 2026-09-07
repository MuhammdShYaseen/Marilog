using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesToDocsAndPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_payments_DocumentId",
                table: "payments");

            migrationBuilder.CreateIndex(
                name: "IX_payments_DocumentId",
                table: "payments",
                column: "DocumentId")
                .Annotation("SqlServer:Include", new[] { "PaidAmount" });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_IsActive_DocDate",
                table: "Documents",
                columns: new[] { "IsActive", "DocDate" })
                .Annotation("SqlServer:Include", new[] { "TotalAmount" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_payments_DocumentId",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "IX_Documents_IsActive_DocDate",
                table: "Documents");

            migrationBuilder.CreateIndex(
                name: "IX_payments_DocumentId",
                table: "payments",
                column: "DocumentId");
        }
    }
}
