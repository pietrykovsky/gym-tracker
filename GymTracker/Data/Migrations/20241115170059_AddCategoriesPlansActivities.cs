using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GymTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoriesPlansActivities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DefaultExerciseExerciseCategory_DefaultExercises_DefaultExe~",
                table: "DefaultExerciseExerciseCategory");

            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseCategoryUserMadeExercise_UserMadeExercises_UserMade~",
                table: "ExerciseCategoryUserMadeExercise");

            migrationBuilder.DropForeignKey(
                name: "FK_UserMadeExercises_AspNetUsers_UserId",
                table: "UserMadeExercises");

            migrationBuilder.DropTable(
                name: "DefaultExercises");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserMadeExercises",
                table: "UserMadeExercises");

            migrationBuilder.RenameTable(
                name: "UserMadeExercises",
                newName: "ExerciseBase");

            migrationBuilder.RenameIndex(
                name: "IX_UserMadeExercises_UserId",
                table: "ExerciseBase",
                newName: "IX_ExerciseBase_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserMadeExercises_Name",
                table: "ExerciseBase",
                newName: "IX_ExerciseBase_Name");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ExerciseBase",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "ExerciseBase",
                type: "character varying(21)",
                maxLength: 21,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExerciseBase",
                table: "ExerciseBase",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "TrainingPlanBase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Discriminator = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingPlanBase", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingPlanBase_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainingPlanCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingPlanCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ActivityBase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    ExerciseId = table.Column<int>(type: "integer", nullable: false),
                    Discriminator = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    PlanId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityBase", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityBase_ExerciseBase_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "ExerciseBase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActivityBase_TrainingPlanBase_PlanId",
                        column: x => x.PlanId,
                        principalTable: "TrainingPlanBase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DefaultTrainingPlanTrainingPlanCategory",
                columns: table => new
                {
                    CategoriesId = table.Column<int>(type: "integer", nullable: false),
                    DefaultTrainingPlansId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefaultTrainingPlanTrainingPlanCategory", x => new { x.CategoriesId, x.DefaultTrainingPlansId });
                    table.ForeignKey(
                        name: "FK_DefaultTrainingPlanTrainingPlanCategory_TrainingPlanBase_De~",
                        column: x => x.DefaultTrainingPlansId,
                        principalTable: "TrainingPlanBase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DefaultTrainingPlanTrainingPlanCategory_TrainingPlanCategor~",
                        column: x => x.CategoriesId,
                        principalTable: "TrainingPlanCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainingPlanCategoryUserMadeTrainingPlan",
                columns: table => new
                {
                    CategoriesId = table.Column<int>(type: "integer", nullable: false),
                    UserMadeTrainingPlansId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingPlanCategoryUserMadeTrainingPlan", x => new { x.CategoriesId, x.UserMadeTrainingPlansId });
                    table.ForeignKey(
                        name: "FK_TrainingPlanCategoryUserMadeTrainingPlan_TrainingPlanBase_U~",
                        column: x => x.UserMadeTrainingPlansId,
                        principalTable: "TrainingPlanBase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrainingPlanCategoryUserMadeTrainingPlan_TrainingPlanCatego~",
                        column: x => x.CategoriesId,
                        principalTable: "TrainingPlanCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseSets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    ActivityId = table.Column<int>(type: "integer", nullable: false),
                    Repetitions = table.Column<int>(type: "integer", nullable: false),
                    Weight = table.Column<float>(type: "real", nullable: true),
                    RestAfterDuration = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExerciseSets_ActivityBase_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "ActivityBase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityBase_ExerciseId",
                table: "ActivityBase",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityBase_PlanId",
                table: "ActivityBase",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_DefaultTrainingPlanTrainingPlanCategory_DefaultTrainingPlan~",
                table: "DefaultTrainingPlanTrainingPlanCategory",
                column: "DefaultTrainingPlansId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseSets_ActivityId",
                table: "ExerciseSets",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingPlanBase_UserId",
                table: "TrainingPlanBase",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingPlanCategories_Name",
                table: "TrainingPlanCategories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrainingPlanCategoryUserMadeTrainingPlan_UserMadeTrainingPl~",
                table: "TrainingPlanCategoryUserMadeTrainingPlan",
                column: "UserMadeTrainingPlansId");

            migrationBuilder.AddForeignKey(
                name: "FK_DefaultExerciseExerciseCategory_ExerciseBase_DefaultExercis~",
                table: "DefaultExerciseExerciseCategory",
                column: "DefaultExercisesId",
                principalTable: "ExerciseBase",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseBase_AspNetUsers_UserId",
                table: "ExerciseBase",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseCategoryUserMadeExercise_ExerciseBase_UserMadeExerc~",
                table: "ExerciseCategoryUserMadeExercise",
                column: "UserMadeExercisesId",
                principalTable: "ExerciseBase",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DefaultExerciseExerciseCategory_ExerciseBase_DefaultExercis~",
                table: "DefaultExerciseExerciseCategory");

            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseBase_AspNetUsers_UserId",
                table: "ExerciseBase");

            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseCategoryUserMadeExercise_ExerciseBase_UserMadeExerc~",
                table: "ExerciseCategoryUserMadeExercise");

            migrationBuilder.DropTable(
                name: "DefaultTrainingPlanTrainingPlanCategory");

            migrationBuilder.DropTable(
                name: "ExerciseSets");

            migrationBuilder.DropTable(
                name: "TrainingPlanCategoryUserMadeTrainingPlan");

            migrationBuilder.DropTable(
                name: "ActivityBase");

            migrationBuilder.DropTable(
                name: "TrainingPlanCategories");

            migrationBuilder.DropTable(
                name: "TrainingPlanBase");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExerciseBase",
                table: "ExerciseBase");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "ExerciseBase");

            migrationBuilder.RenameTable(
                name: "ExerciseBase",
                newName: "UserMadeExercises");

            migrationBuilder.RenameIndex(
                name: "IX_ExerciseBase_UserId",
                table: "UserMadeExercises",
                newName: "IX_UserMadeExercises_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_ExerciseBase_Name",
                table: "UserMadeExercises",
                newName: "IX_UserMadeExercises_Name");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserMadeExercises",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserMadeExercises",
                table: "UserMadeExercises",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "DefaultExercises",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Difficulty = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefaultExercises", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DefaultExercises_Name",
                table: "DefaultExercises",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DefaultExerciseExerciseCategory_DefaultExercises_DefaultExe~",
                table: "DefaultExerciseExerciseCategory",
                column: "DefaultExercisesId",
                principalTable: "DefaultExercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseCategoryUserMadeExercise_UserMadeExercises_UserMade~",
                table: "ExerciseCategoryUserMadeExercise",
                column: "UserMadeExercisesId",
                principalTable: "UserMadeExercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserMadeExercises_AspNetUsers_UserId",
                table: "UserMadeExercises",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
