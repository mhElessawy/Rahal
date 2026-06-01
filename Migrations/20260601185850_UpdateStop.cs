using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RahalWeb.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ContractSuspend",
                table: "PasswordData",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContractSuspend",
                table: "PasswordData");
        }
    }
}
