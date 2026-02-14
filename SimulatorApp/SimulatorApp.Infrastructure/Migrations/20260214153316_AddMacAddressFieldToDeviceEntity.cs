using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimulatorApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMacAddressFieldToDeviceEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MAC",
                table: "Devices",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MAC",
                table: "Devices");
        }
    }
}
