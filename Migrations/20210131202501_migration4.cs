using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace InternetSecurityProject.Migrations
{
    public partial class migration4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tfa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    IsPasswordOk = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsCertificateOk = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsTokenOk = table.Column<bool>(type: "INTEGER", nullable: false),
                    FirstFactorTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tfa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tfa_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tfa_UserId",
                table: "tfa",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tfa");
        }
    }
}
