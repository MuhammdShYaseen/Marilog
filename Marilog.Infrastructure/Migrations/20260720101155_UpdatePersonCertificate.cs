using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePersonCertificate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PersonCertificates",
                table: "PersonCertificates");

            migrationBuilder.RenameColumn(
                name: "IssuingAuthority",
                table: "PersonCertificates",
                newName: "Certificate_IssuingAuthority");

            migrationBuilder.RenameColumn(
                name: "IssueDate",
                table: "PersonCertificates",
                newName: "Certificate_IssueDate");

            migrationBuilder.RenameColumn(
                name: "ExpiryDate",
                table: "PersonCertificates",
                newName: "Certificate_ExpiryDate");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "PersonCertificates",
                newName: "Certificate_Description");

            migrationBuilder.RenameColumn(
                name: "CertificateNumber",
                table: "PersonCertificates",
                newName: "Certificate_CertificateNumber");

            migrationBuilder.RenameColumn(
                name: "CertificateName",
                table: "PersonCertificates",
                newName: "Certificate_CertificateName");

            migrationBuilder.AlterColumn<int>(
                name: "PersonId",
                table: "PersonCertificates",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "PersonCertificates",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "Guid",
                table: "PersonCertificates",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "PersonCertificates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "PersonCertificates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "PersonCertificates",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "PersonCertificates",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PersonCertificates",
                table: "PersonCertificates",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_PersonCertificates_Guid",
                table: "PersonCertificates",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonCertificates_PersonId",
                table: "PersonCertificates",
                column: "PersonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PersonCertificates",
                table: "PersonCertificates");

            migrationBuilder.DropIndex(
                name: "IX_PersonCertificates_Guid",
                table: "PersonCertificates");

            migrationBuilder.DropIndex(
                name: "IX_PersonCertificates_PersonId",
                table: "PersonCertificates");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "PersonCertificates");

            migrationBuilder.DropColumn(
                name: "Guid",
                table: "PersonCertificates");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "PersonCertificates");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "PersonCertificates");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "PersonCertificates");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "PersonCertificates");

            migrationBuilder.RenameColumn(
                name: "Certificate_IssuingAuthority",
                table: "PersonCertificates",
                newName: "IssuingAuthority");

            migrationBuilder.RenameColumn(
                name: "Certificate_IssueDate",
                table: "PersonCertificates",
                newName: "IssueDate");

            migrationBuilder.RenameColumn(
                name: "Certificate_ExpiryDate",
                table: "PersonCertificates",
                newName: "ExpiryDate");

            migrationBuilder.RenameColumn(
                name: "Certificate_Description",
                table: "PersonCertificates",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "Certificate_CertificateNumber",
                table: "PersonCertificates",
                newName: "CertificateNumber");

            migrationBuilder.RenameColumn(
                name: "Certificate_CertificateName",
                table: "PersonCertificates",
                newName: "CertificateName");

            migrationBuilder.AlterColumn<int>(
                name: "PersonId",
                table: "PersonCertificates",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PersonCertificates",
                table: "PersonCertificates",
                columns: new[] { "PersonId", "Id" });
        }
    }
}
