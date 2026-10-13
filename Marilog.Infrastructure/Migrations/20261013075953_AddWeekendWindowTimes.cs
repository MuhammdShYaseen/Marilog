using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marilog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWeekendWindowTimes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "LaytimeTerms_RuleOptions_WeekendEndTime",
                table: "CharterTerms",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "LaytimeTerms_RuleOptions_WeekendStartTime",
                table: "CharterTerms",
                type: "time",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LaytimeTerms_RuleOptions_WeekendEndTime",
                table: "CharterTerms");

            migrationBuilder.DropColumn(
                name: "LaytimeTerms_RuleOptions_WeekendStartTime",
                table: "CharterTerms");
        }
    }
}
