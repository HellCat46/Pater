using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthActionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuthAction",
                columns: table => new
                {
                    code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    action = table.Column<int>(type: "int", nullable: false),
                    Userid = table.Column<int>(type: "int", nullable: false),
                    createAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthAction", x => x.code);
                    table.ForeignKey(
                        name: "FK_AuthAction_Account_Userid",
                        column: x => x.Userid,
                        principalTable: "Account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthAction_Userid",
                table: "AuthAction",
                column: "Userid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthAction");
        }
    }
}
