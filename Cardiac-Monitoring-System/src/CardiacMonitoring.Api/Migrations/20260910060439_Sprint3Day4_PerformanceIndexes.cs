using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CardiacMonitoring.Api.Migrations
{
    /// <inheritdoc />
    public partial class Sprint3Day4_PerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VitalSigns_PatientId",
                table: "VitalSigns");

            migrationBuilder.AlterColumn<string>(
                name: "RiskLevel",
                table: "VitalSigns",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_VitalSigns_PatientId_RecordedAtUtc",
                table: "VitalSigns",
                columns: new[] { "PatientId", "RecordedAtUtc" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_VitalSigns_RiskLevel_RecordedAtUtc",
                table: "VitalSigns",
                columns: new[] { "RiskLevel", "RecordedAtUtc" },
                descending: new[] { false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VitalSigns_PatientId_RecordedAtUtc",
                table: "VitalSigns");

            migrationBuilder.DropIndex(
                name: "IX_VitalSigns_RiskLevel_RecordedAtUtc",
                table: "VitalSigns");

            migrationBuilder.AlterColumn<string>(
                name: "RiskLevel",
                table: "VitalSigns",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_VitalSigns_PatientId",
                table: "VitalSigns",
                column: "PatientId");
        }
    }
}
