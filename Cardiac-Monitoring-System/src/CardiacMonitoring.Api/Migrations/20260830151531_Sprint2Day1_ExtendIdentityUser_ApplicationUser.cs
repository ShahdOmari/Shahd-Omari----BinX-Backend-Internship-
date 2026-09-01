using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CardiacMonitoring.Api.Migrations
{
    /// <inheritdoc />
    public partial class Sprint2Day1_ExtendIdentityUser_ApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LicenseNumber",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a1111111-1111-1111-1111-111111111111",
                column: "ConcurrencyStamp",
                value: "c1111111-1111-1111-1111-111111111111");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b2222222-2222-2222-2222-222222222222",
                column: "ConcurrencyStamp",
                value: "c2222222-2222-2222-2222-222222222222");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LicenseNumber",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a1111111-1111-1111-1111-111111111111",
                column: "ConcurrencyStamp",
                value: "f170be54-276c-4e8d-a199-73a3617624c9");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b2222222-2222-2222-2222-222222222222",
                column: "ConcurrencyStamp",
                value: "b2df0e56-6b35-46a7-ae8f-49ed6f622185");
        }
    }
}
