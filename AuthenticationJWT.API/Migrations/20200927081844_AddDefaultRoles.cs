using Microsoft.EntityFrameworkCore.Migrations;

namespace AuthenticationJWT.API.Migrations
{
    public partial class AddDefaultRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "4c64ccf3-f112-4977-89e0-0ea2de73ba27", "10ca628f-90f6-4d7b-b12a-5beb0a8a2861", "Manager", "MANAGER" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "259e06bb-c271-45fc-8f8d-fced48912b28", "24de5df1-4a2d-41b6-bf42-112a673bb0a7", "Administrator", "ADMINISTRATOR" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "259e06bb-c271-45fc-8f8d-fced48912b28");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "4c64ccf3-f112-4977-89e0-0ea2de73ba27");
        }
    }
}
