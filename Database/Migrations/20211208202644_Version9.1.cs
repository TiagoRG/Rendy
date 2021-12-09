using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class Version91 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MuteId",
                table: "Servers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MuteId",
                table: "Servers");
        }
    }
}
