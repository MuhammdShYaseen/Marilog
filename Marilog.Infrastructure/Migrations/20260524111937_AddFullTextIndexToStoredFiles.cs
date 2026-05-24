using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFullTextIndexToStoredFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {


            migrationBuilder.Sql(@"
        CREATE FULLTEXT INDEX ON StoredFiles(Content)
        KEY INDEX PK_StoredFiles
        ON StoredFilesCatalog
        WITH CHANGE_TRACKING AUTO;
    ", suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        DROP FULLTEXT INDEX ON StoredFiles;
    ", suppressTransaction: true);

            migrationBuilder.Sql(@"
        DROP FULLTEXT CATALOG StoredFilesCatalog;
    ", suppressTransaction: true);
        }
    }
}