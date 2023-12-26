using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class UpdatesAnalyticsModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Analytics",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    browser = table.Column<int>(type: "int", nullable: false),
                    device = table.Column<int>(type: "int", nullable: false),
                    os = table.Column<int>(type: "int", nullable: false),
                    country = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    city = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    visitedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LinkModelCode = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Analytics", x => x.id);
                    table.ForeignKey(
                        name: "FK_Analytics_Link_LinkModelCode",
                        column: x => x.LinkModelCode,
                        principalTable: "Link",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Analytics_LinkModelCode",
                table: "Analytics",
                column: "LinkModelCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Analytics");
        }
    }
}
