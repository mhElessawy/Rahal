using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RahalWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddStopReasonToContractDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StopReason",
                table: "ContractDetails",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StopReason",
                table: "ContractDetails");
        }
    }
}
