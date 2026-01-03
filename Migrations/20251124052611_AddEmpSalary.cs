using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RahalWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddEmpSalary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {


         

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSalary_EmpId",
                table: "EmployeeSalary",
                column: "EmpId");

          
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CarInfoAtt");

            migrationBuilder.DropTable(
                name: "CompanyDebitDetails");

            migrationBuilder.DropTable(
                name: "CompanyInfoAtt");

            migrationBuilder.DropTable(
                name: "ContractDetails");

            migrationBuilder.DropTable(
                name: "CreditBill");

            migrationBuilder.DropTable(
                name: "DebitPayInfo");

            migrationBuilder.DropTable(
                name: "DeffInformation");

            migrationBuilder.DropTable(
                name: "EmployeeInfoAtt");

            migrationBuilder.DropTable(
                name: "EmployeeSalary");

            migrationBuilder.DropTable(
                name: "EmployeeTakeMoney");

            migrationBuilder.DropTable(
                name: "Purshase");

            migrationBuilder.DropTable(
                name: "UserCompanyNotAppear");

            migrationBuilder.DropTable(
                name: "Vacation");

            migrationBuilder.DropTable(
                name: "CompanyDebit");

            migrationBuilder.DropTable(
                name: "Bill");

            migrationBuilder.DropTable(
                name: "DebitInfo");

            migrationBuilder.DropTable(
                name: "ViolationInfo");

            migrationBuilder.DropTable(
                name: "Contract");

            migrationBuilder.DropTable(
                name: "CarInfo");

            migrationBuilder.DropTable(
                name: "EmployeeInfo");

            migrationBuilder.DropTable(
                name: "CompanyInfo");

            migrationBuilder.DropTable(
                name: "PasswordData");

            migrationBuilder.DropTable(
                name: "Deff");

            migrationBuilder.DropTable(
                name: "DeffType");
        }
    }
}
