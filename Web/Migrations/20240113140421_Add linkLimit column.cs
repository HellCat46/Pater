using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddlinkLimitcolumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Plan",
                table: "Account",
                newName: "plan");

            migrationBuilder.RenameColumn(
                name: "PicPath",
                table: "Account",
                newName: "picPath");

            migrationBuilder.AddColumn<int>(
                name: "linkLimit",
                table: "Account",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "linkLimit",
                table: "Account");

            migrationBuilder.RenameColumn(
                name: "plan",
                table: "Account",
                newName: "Plan");

            migrationBuilder.RenameColumn(
                name: "picPath",
                table: "Account",
                newName: "PicPath");
        }
    }
}
