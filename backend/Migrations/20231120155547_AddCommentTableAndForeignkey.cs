using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    public partial class AddCommentTableAndForeignkey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "userId",
                table: "Comment",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Comment_ExerciseId",
                table: "Comment",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_Comment_userId",
                table: "Comment",
                column: "userId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_AspNetUsers_userId",
                table: "Comment",
                column: "userId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_Exercise_ExerciseId",
                table: "Comment",
                column: "ExerciseId",
                principalTable: "Exercise",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comment_AspNetUsers_userId",
                table: "Comment");

            migrationBuilder.DropForeignKey(
                name: "FK_Comment_Exercise_ExerciseId",
                table: "Comment");

            migrationBuilder.DropIndex(
                name: "IX_Comment_ExerciseId",
                table: "Comment");

            migrationBuilder.DropIndex(
                name: "IX_Comment_userId",
                table: "Comment");

            migrationBuilder.AlterColumn<string>(
                name: "userId",
                table: "Comment",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
