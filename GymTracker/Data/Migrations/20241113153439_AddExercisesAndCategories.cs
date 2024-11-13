using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GymTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddExercisesAndCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DefaultExercises",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Difficulty = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefaultExercises", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserMadeExercises",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Difficulty = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMadeExercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserMadeExercises_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DefaultExerciseExerciseCategory",
                columns: table => new
                {
                    CategoriesId = table.Column<int>(type: "integer", nullable: false),
                    DefaultExercisesId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefaultExerciseExerciseCategory", x => new { x.CategoriesId, x.DefaultExercisesId });
                    table.ForeignKey(
                        name: "FK_DefaultExerciseExerciseCategory_DefaultExercises_DefaultExe~",
                        column: x => x.DefaultExercisesId,
                        principalTable: "DefaultExercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DefaultExerciseExerciseCategory_ExerciseCategories_Categori~",
                        column: x => x.CategoriesId,
                        principalTable: "ExerciseCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseCategoryUserMadeExercise",
                columns: table => new
                {
                    CategoriesId = table.Column<int>(type: "integer", nullable: false),
                    UserMadeExercisesId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseCategoryUserMadeExercise", x => new { x.CategoriesId, x.UserMadeExercisesId });
                    table.ForeignKey(
                        name: "FK_ExerciseCategoryUserMadeExercise_ExerciseCategories_Categor~",
                        column: x => x.CategoriesId,
                        principalTable: "ExerciseCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExerciseCategoryUserMadeExercise_UserMadeExercises_UserMade~",
                        column: x => x.UserMadeExercisesId,
                        principalTable: "UserMadeExercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DefaultExerciseExerciseCategory_DefaultExercisesId",
                table: "DefaultExerciseExerciseCategory",
                column: "DefaultExercisesId");

            migrationBuilder.CreateIndex(
                name: "IX_DefaultExercises_Name",
                table: "DefaultExercises",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseCategories_Name",
                table: "ExerciseCategories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseCategoryUserMadeExercise_UserMadeExercisesId",
                table: "ExerciseCategoryUserMadeExercise",
                column: "UserMadeExercisesId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMadeExercises_Name",
                table: "UserMadeExercises",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserMadeExercises_UserId",
                table: "UserMadeExercises",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DefaultExerciseExerciseCategory");

            migrationBuilder.DropTable(
                name: "ExerciseCategoryUserMadeExercise");

            migrationBuilder.DropTable(
                name: "DefaultExercises");

            migrationBuilder.DropTable(
                name: "ExerciseCategories");

            migrationBuilder.DropTable(
                name: "UserMadeExercises");
        }
    }
}
