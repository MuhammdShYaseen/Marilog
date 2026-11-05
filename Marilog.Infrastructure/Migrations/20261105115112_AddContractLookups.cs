using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContractLookups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ContractParties",
                table: "ContractParties");

            migrationBuilder.DropColumn(
                name: "ContractFileName",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "ContractFileUrl",
                table: "Contracts");

            // ✏️ 1: DropColumn "Role" moved below — the old role values are copied into RoleId first.

            migrationBuilder.AddColumn<int>(
                name: "RoleId",
                table: "ContractParties",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // ✏️ 2: copy old enum names into RoleId (Ids match the seed below), then drop Role.
            migrationBuilder.Sql(@"
UPDATE ContractParties SET RoleId = CASE Role
  WHEN 'Owner' THEN 1 WHEN 'Charterer' THEN 2 WHEN 'Agent' THEN 3
  WHEN 'Supplier' THEN 4 WHEN 'Guarantor' THEN 5 END;");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "ContractParties");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContractParties",
                table: "ContractParties",
                columns: new[] { "ContractId", "CompanyId", "RoleId" });

            migrationBuilder.CreateTable(
                name: "ContractRoles",
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
                    table.PrimaryKey("PK_ContractRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContractTypes",
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
                    table.PrimaryKey("PK_ContractTypes", x => x.Id);
                });

            // ✏️ 3: seed with the old enum ids so existing Contracts.TypeId and ContractParties.RoleId match before the FKs are added.
            migrationBuilder.Sql(@"
SET IDENTITY_INSERT ContractTypes ON;
INSERT INTO ContractTypes (Id, Code, Name, SortOrder, IsActive, IsDeleted, Guid, CreatedAt) VALUES
 (1, 'CP', 'Charter Party', 1, 1, 0, NEWID(), GETDATE()),
 (3, 'SUPPLIER', 'Supplier', 2, 1, 0, NEWID(), GETDATE()),
 (4, 'AGENCY', 'Agency', 3, 1, 0, NEWID(), GETDATE());
SET IDENTITY_INSERT ContractTypes OFF;

SET IDENTITY_INSERT ContractRoles ON;
INSERT INTO ContractRoles (Id, Code, Name, SortOrder, IsActive, IsDeleted, Guid, CreatedAt) VALUES
 (1, 'OWNER', 'Owner', 1, 1, 0, NEWID(), GETDATE()),
 (2, 'CHRT', 'Charterer', 2, 1, 0, NEWID(), GETDATE()),
 (3, 'AGENT', 'Agent', 3, 1, 0, NEWID(), GETDATE()),
 (4, 'SUPPLIER', 'Supplier', 4, 1, 0, NEWID(), GETDATE()),
 (5, 'GUARANTOR', 'Guarantor', 5, 1, 0, NEWID(), GETDATE());
SET IDENTITY_INSERT ContractRoles OFF;");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_TypeId",
                table: "Contracts",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractParties_RoleId",
                table: "ContractParties",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractRoles_Code",
                table: "ContractRoles",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractRoles_Guid",
                table: "ContractRoles",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractTypes_Code",
                table: "ContractTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractTypes_Guid",
                table: "ContractTypes",
                column: "Guid",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ContractParties_ContractRoles_RoleId",
                table: "ContractParties",
                column: "RoleId",
                principalTable: "ContractRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_ContractTypes_TypeId",
                table: "Contracts",
                column: "TypeId",
                principalTable: "ContractTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContractParties_ContractRoles_RoleId",
                table: "ContractParties");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_ContractTypes_TypeId",
                table: "Contracts");

            migrationBuilder.DropTable(
                name: "ContractRoles");

            migrationBuilder.DropTable(
                name: "ContractTypes");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_TypeId",
                table: "Contracts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ContractParties",
                table: "ContractParties");

            migrationBuilder.DropIndex(
                name: "IX_ContractParties_RoleId",
                table: "ContractParties");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "ContractParties");

            migrationBuilder.AddColumn<string>(
                name: "ContractFileName",
                table: "Contracts",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContractFileUrl",
                table: "Contracts",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "ContractParties",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContractParties",
                table: "ContractParties",
                columns: new[] { "ContractId", "CompanyId", "Role" });
        }
    }
}