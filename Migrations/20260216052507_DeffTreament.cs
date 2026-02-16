using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RahalWeb.Migrations
{
    /// <inheritdoc />
    public partial class DeffTreament : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EntrySalaryDay",
                table: "EmployeeSalary",
                newName: "UserId");

            migrationBuilder.AddColumn<bool>(
                name: "DebitUpdate",
                table: "PasswordData",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "SalaryRecieved",
                table: "EmployeeSalary",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<int>(
                name: "Approve",
                table: "EmployeeSalary",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.CreateTable(
                name: "DeffEmpTreatment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    DeffCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeffName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Price1 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Price2 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Price3 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DeleteFlag = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeffEmpTreatment", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSalary_UserId",
                table: "EmployeeSalary",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeSalary_PasswordData",
                table: "EmployeeSalary",
                column: "UserId",
                principalTable: "PasswordData",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeSalary_PasswordData",
                table: "EmployeeSalary");

            migrationBuilder.DropTable(
                name: "DeffEmpTreatment");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeSalary_UserId",
                table: "EmployeeSalary");

            migrationBuilder.DropColumn(
                name: "DebitUpdate",
                table: "PasswordData");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "EmployeeSalary",
                newName: "EntrySalaryDay");

            migrationBuilder.AlterColumn<decimal>(
                name: "SalaryRecieved",
                table: "EmployeeSalary",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "Approve",
                table: "EmployeeSalary",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
