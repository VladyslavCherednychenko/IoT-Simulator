using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimulatorApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveThresholdFieldForAlerts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ThresholdValue",
                table: "AlertRules");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ThresholdValue",
                table: "AlertRules",
                type: "double precision",
                nullable: true);
        }
    }
}
