using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    public partial class UpdateForeignKeyCommentTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comment_AspNetUsers_userId",
                table: "Comment");

            migrationBuilder.RenameColumn(
                name: "userId",
                table: "Comment",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Comment_userId",
                table: "Comment",
                newName: "IX_Comment_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_AspNetUsers_UserId",
                table: "Comment",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comment_AspNetUsers_UserId",
                table: "Comment");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Comment",
                newName: "userId");

            migrationBuilder.RenameIndex(
                name: "IX_Comment_UserId",
                table: "Comment",
                newName: "IX_Comment_userId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_AspNetUsers_userId",
                table: "Comment",
                column: "userId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
