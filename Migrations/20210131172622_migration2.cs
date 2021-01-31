using Microsoft.EntityFrameworkCore.Migrations;

namespace InternetSecurityProject.Migrations
{
    public partial class migration2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_certificate_user_UserId",
                table: "certificate");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "certificate",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_certificate_user_UserId",
                table: "certificate",
                column: "UserId",
                principalTable: "user",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_certificate_user_UserId",
                table: "certificate");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "certificate",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_certificate_user_UserId",
                table: "certificate",
                column: "UserId",
                principalTable: "user",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
