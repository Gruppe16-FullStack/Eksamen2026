using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pendlerapp.Migrations
{
    /// <inheritdoc />
    public partial class AddPlanlagtAvgangstid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PlanlagtAvgangstid",
                table: "Reisehistorikker",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlanlagtAvgangstid",
                table: "Reisehistorikker");
        }
    }
}
