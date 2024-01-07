using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    public partial class AddTableTestCaseAddFk : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TestCases_ExerciseId",
                table: "TestCases",
                column: "ExerciseId");

            migrationBuilder.AddForeignKey(
                name: "FK_TestCases_Exercise_ExerciseId",
                table: "TestCases",
                column: "ExerciseId",
                principalTable: "Exercise",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestCases_Exercise_ExerciseId",
                table: "TestCases");

            migrationBuilder.DropIndex(
                name: "IX_TestCases_ExerciseId",
                table: "TestCases");
        }
    }
}
