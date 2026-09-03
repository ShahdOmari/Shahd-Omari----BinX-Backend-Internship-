using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CardiacMonitoring.Api.Migrations
{
    /// <inheritdoc />
    public partial class Sprint2Day3_RecordedByStaffProfileId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RecordedByStaffProfileId",
                table: "VitalSigns",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecordedByStaffProfileId",
                table: "VitalSigns");
        }
    }
}
