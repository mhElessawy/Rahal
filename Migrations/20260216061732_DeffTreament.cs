using System;
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
            migrationBuilder.CreateTable(
                name: "DeffEmpTreatment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeffCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeffName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Price1 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Price2 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Price3 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DeleteFlag = table.Column<int>(type: "int", nullable: true, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeffEmpTreatment", x => x.Id);
                });

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
          

            migrationBuilder.DropTable(
                name: "DeffEmpTreatment");

        }
    }
}
