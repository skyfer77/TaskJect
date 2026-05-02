using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddForeignKeyOnApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "PersonalTodo",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "PersonalNote",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalTodo_UserId",
                table: "PersonalTodo",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalNote_UserId",
                table: "PersonalNote",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonalNote_AspNetUsers_UserId",
                table: "PersonalNote",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PersonalTodo_AspNetUsers_UserId",
                table: "PersonalTodo",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PersonalNote_AspNetUsers_UserId",
                table: "PersonalNote");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonalTodo_AspNetUsers_UserId",
                table: "PersonalTodo");

            migrationBuilder.DropIndex(
                name: "IX_PersonalTodo_UserId",
                table: "PersonalTodo");

            migrationBuilder.DropIndex(
                name: "IX_PersonalNote_UserId",
                table: "PersonalNote");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "PersonalTodo",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "PersonalNote",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
