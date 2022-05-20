using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prinubes.PlatformWorker.Migrations
{
    public partial class platformchanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Enabled",
                table: "networkplatforms",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "message",
                table: "networkplatforms",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "state",
                table: "networkplatforms",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Enabled",
                table: "loadbalancerplatforms",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "message",
                table: "loadbalancerplatforms",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "state",
                table: "loadbalancerplatforms",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Enabled",
                table: "computeplatforms",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "message",
                table: "computeplatforms",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "state",
                table: "computeplatforms",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Enabled",
                table: "networkplatforms");

            migrationBuilder.DropColumn(
                name: "message",
                table: "networkplatforms");

            migrationBuilder.DropColumn(
                name: "state",
                table: "networkplatforms");

            migrationBuilder.DropColumn(
                name: "Enabled",
                table: "loadbalancerplatforms");

            migrationBuilder.DropColumn(
                name: "message",
                table: "loadbalancerplatforms");

            migrationBuilder.DropColumn(
                name: "state",
                table: "loadbalancerplatforms");

            migrationBuilder.DropColumn(
                name: "Enabled",
                table: "computeplatforms");

            migrationBuilder.DropColumn(
                name: "message",
                table: "computeplatforms");

            migrationBuilder.DropColumn(
                name: "state",
                table: "computeplatforms");
        }
    }
}
