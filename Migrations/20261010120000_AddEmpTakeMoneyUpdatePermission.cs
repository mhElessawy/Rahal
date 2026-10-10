using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RahalWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddEmpTakeMoneyUpdatePermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EmpTakeMoneyUpdate",
                table: "PasswordData",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // المستخدمين اللي كان معاهم صلاحية حفظ العهد كانوا بيقدروا يعدلوا، فنحافظ على نفس الصلاحية ليهم
            migrationBuilder.Sql("UPDATE PasswordData SET EmpTakeMoneyUpdate = EmpTakeMoneySave");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmpTakeMoneyUpdate",
                table: "PasswordData");
        }
    }
}
