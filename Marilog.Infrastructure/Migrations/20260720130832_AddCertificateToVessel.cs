using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCertificateToVessel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VesselCertificates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Certificate_CertificateName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Certificate_CertificateNumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Certificate_IssuingAuthority = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Certificate_IssueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Certificate_ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Certificate_Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    VesselId = table.Column<int>(type: "int", nullable: true),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
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
                name: "IX_VesselCertificates_Guid",
                table: "VesselCertificates",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VesselCertificates_VesselId",
                table: "VesselCertificates",
                column: "VesselId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VesselCertificates");
        }
    }
}
