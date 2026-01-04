using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Workshop1.Migrations
{
    /// <inheritdoc />
    public partial class LinkTeacherToIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Teachers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TeacherId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Teachers",
                keyColumn: "Id",
                keyValue: 1,
                column: "ApplicationUserId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Teachers",
                keyColumn: "Id",
                keyValue: 2,
                column: "ApplicationUserId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Teachers",
                keyColumn: "Id",
                keyValue: 3,
                column: "ApplicationUserId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Teachers",
                keyColumn: "Id",
                keyValue: 4,
                column: "ApplicationUserId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Teachers",
                keyColumn: "Id",
                keyValue: 5,
                column: "ApplicationUserId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_ApplicationUserId",
                table: "Teachers",
                column: "ApplicationUserId",
                unique: true,
                filter: "[ApplicationUserId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Teachers_AspNetUsers_ApplicationUserId",
                table: "Teachers",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teachers_AspNetUsers_ApplicationUserId",
                table: "Teachers");

            migrationBuilder.DropIndex(
                name: "IX_Teachers_ApplicationUserId",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "TeacherId",
                table: "AspNetUsers");
        }
    }
}
