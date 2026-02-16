using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RahalWeb.Migrations
{
    /// <inheritdoc />
    public partial class EmpTreatment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmpTreatments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpId = table.Column<int>(type: "int", nullable: true),
                    DeffEmpTreatmentId = table.Column<int>(type: "int", nullable: true),
                    TreatmentNo = table.Column<int>(type: "int", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TreatmentDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TreatmentExtraMoney = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TreatmentTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DeleteFlag = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    UserRecievedId = table.Column<int>(type: "int", nullable: true),
                    UserRecievedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    UserRecievedNo = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpTreatments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmpTreatments_DeffEmpTreatment_DeffEmpTreatmentId",
                        column: x => x.DeffEmpTreatmentId,
                        principalTable: "DeffEmpTreatment",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmpTreatments_EmployeeInfo_EmpId",
                        column: x => x.EmpId,
                        principalTable: "EmployeeInfo",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmpTreatments_PasswordData_UserId",
                        column: x => x.UserId,
                        principalTable: "PasswordData",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmpTreatments_PasswordData_UserRecievedId",
                        column: x => x.UserRecievedId,
                        principalTable: "PasswordData",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmpTreatments_DeffEmpTreatmentId",
                table: "EmpTreatments",
                column: "DeffEmpTreatmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EmpTreatments_EmpId",
                table: "EmpTreatments",
                column: "EmpId");

            migrationBuilder.CreateIndex(
                name: "IX_EmpTreatments_UserId",
                table: "EmpTreatments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmpTreatments_UserRecievedId",
                table: "EmpTreatments",
                column: "UserRecievedId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmpTreatments");
        }
    }
}
