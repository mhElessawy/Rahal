using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RahalWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddEmpTakeMoneyPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EmpTakeMoneySave",
                table: "PasswordData",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EmpTakeMoneyDelete",
                table: "PasswordData",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmpTakeMoneySave",
                table: "PasswordData");

            migrationBuilder.DropColumn(
                name: "EmpTakeMoneyDelete",
                table: "PasswordData");
        }
    }
}