using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class Version8 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InviteBlocker",
                table: "Servers");

            migrationBuilder.DropColumn(
                name: "Punishment",
                table: "Servers");

            migrationBuilder.AddColumn<ulong>(
                name: "Logs",
                table: "Servers",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.CreateTable(
                name: "ModSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ServerId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    InviteBlocker = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Punishment = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModSettings", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModSettings");

            migrationBuilder.DropColumn(
                name: "Logs",
                table: "Servers");

            migrationBuilder.AddColumn<bool>(
                name: "InviteBlocker",
                table: "Servers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Punishment",
                table: "Servers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
