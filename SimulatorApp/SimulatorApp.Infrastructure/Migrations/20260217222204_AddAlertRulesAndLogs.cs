using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SimulatorApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertRulesAndLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlertRules",
                columns: table => new
                {
                    AlertRuleId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SensorId = table.Column<long>(type: "bigint", nullable: false),
                    AlertType = table.Column<int>(type: "integer", nullable: false),
                    ThresholdValue = table.Column<double>(type: "double precision", nullable: true),
                    RangeMin = table.Column<double>(type: "double precision", nullable: true),
                    RangeMax = table.Column<double>(type: "double precision", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertRules", x => x.AlertRuleId);
                    table.ForeignKey(
                        name: "FK_AlertRules_Sensors_SensorId",
                        column: x => x.SensorId,
                        principalTable: "Sensors",
                        principalColumn: "SensorId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlertLogs",
                columns: table => new
                {
                    AlertLogId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AlertRuleId = table.Column<long>(type: "bigint", nullable: false),
                    SensorId = table.Column<long>(type: "bigint", nullable: false),
                    TriggerValue = table.Column<double>(type: "double precision", nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertLogs", x => x.AlertLogId);
                    table.ForeignKey(
                        name: "FK_AlertLogs_AlertRules_AlertRuleId",
                        column: x => x.AlertRuleId,
                        principalTable: "AlertRules",
                        principalColumn: "AlertRuleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlertLogs_Sensors_SensorId",
                        column: x => x.SensorId,
                        principalTable: "Sensors",
                        principalColumn: "SensorId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlertLogs_AlertRuleId",
                table: "AlertLogs",
                column: "AlertRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_AlertLogs_SensorId_Timestamp",
                table: "AlertLogs",
                columns: new[] { "SensorId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_AlertRules_SensorId",
                table: "AlertRules",
                column: "SensorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertLogs");

            migrationBuilder.DropTable(
                name: "AlertRules");
        }
    }
}
