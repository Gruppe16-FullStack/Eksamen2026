using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pendlerapp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Favoritter",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Navn = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    FraStoppested = table.Column<string>(type: "TEXT", nullable: false),
                    FraStoppestedId = table.Column<string>(type: "TEXT", nullable: false),
                    TilStoppested = table.Column<string>(type: "TEXT", nullable: false),
                    TilStoppestedId = table.Column<string>(type: "TEXT", nullable: false),
                    Opprettet = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BrukerId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favoritter", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reisehistorikker",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FavorittId = table.Column<int>(type: "INTEGER", nullable: false),
                    Brukt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FaktiskAvgangstid = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reisehistorikker", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reisehistorikker_Favoritter_FavorittId",
                        column: x => x.FavorittId,
                        principalTable: "Favoritter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reisehistorikker_FavorittId",
                table: "Reisehistorikker",
                column: "FavorittId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reisehistorikker");

            migrationBuilder.DropTable(
                name: "Favoritter");
        }
    }
}
