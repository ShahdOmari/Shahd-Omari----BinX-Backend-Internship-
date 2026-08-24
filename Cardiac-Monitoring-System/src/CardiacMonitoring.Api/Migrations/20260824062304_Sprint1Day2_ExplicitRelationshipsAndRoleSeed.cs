using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CardiacMonitoring.Api.Migrations
{
    /// <inheritdoc />
    public partial class Sprint1Day2_ExplicitRelationshipsAndRoleSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "a1111111-1111-1111-1111-111111111111", "f170be54-276c-4e8d-a199-73a3617624c9", "Nurse", "NURSE" },
                    { "b2222222-2222-2222-2222-222222222222", "b2df0e56-6b35-46a7-ae8f-49ed6f622185", "Doctor", "DOCTOR" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a1111111-1111-1111-1111-111111111111");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b2222222-2222-2222-2222-222222222222");
        }
    }
}
