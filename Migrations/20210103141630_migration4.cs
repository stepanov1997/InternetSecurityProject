using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace InternetSecurityProject.Migrations
{
    public partial class migration4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Seen",
                table: "message");

            migrationBuilder.AddColumn<DateTime>(
                name: "SeenDateTime",
                table: "message",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SeenDateTime",
                table: "message");

            migrationBuilder.AddColumn<bool>(
                name: "Seen",
                table: "message",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
