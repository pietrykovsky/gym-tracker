using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddEquipmentAndMovementTypeToExerciseBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PrimaryCategoryId",
                table: "ExerciseBase",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RequiredEquipment",
                table: "ExerciseBase",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseBase_PrimaryCategoryId",
                table: "ExerciseBase",
                column: "PrimaryCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseBase_ExerciseCategories_PrimaryCategoryId",
                table: "ExerciseBase",
                column: "PrimaryCategoryId",
                principalTable: "ExerciseCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseBase_ExerciseCategories_PrimaryCategoryId",
                table: "ExerciseBase");

            migrationBuilder.DropIndex(
                name: "IX_ExerciseBase_PrimaryCategoryId",
                table: "ExerciseBase");

            migrationBuilder.DropColumn(
                name: "PrimaryCategoryId",
                table: "ExerciseBase");

            migrationBuilder.DropColumn(
                name: "RequiredEquipment",
                table: "ExerciseBase");
        }
    }
}
