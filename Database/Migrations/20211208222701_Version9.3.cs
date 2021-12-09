using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class Version93 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MuteId",
                table: "Mutes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MuteId",
                table: "Mutes");
        }
    }
}
