using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoneySaver.Identity.Migrations
{
    public partial class Add_User_State_Property : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "State",
                table: "AspNetUsers");
        }
    }
}
