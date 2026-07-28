using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAttachmentFromEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmailAttachments_Emails_EmailId",
                table: "EmailAttachments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_EmailAttachments_Emails_EmailId",
                table: "EmailAttachments",
                column: "EmailId",
                principalTable: "Emails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
