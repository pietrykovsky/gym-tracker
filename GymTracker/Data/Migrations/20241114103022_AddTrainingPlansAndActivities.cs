using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GymTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainingPlansAndActivities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserMadeExercises_AspNetUsers_UserId",
                table: "UserMadeExercises");

            migrationBuilder.DropTable(
                name: "DefaultExerciseExerciseCategory");

            migrationBuilder.DropTable(
                name: "ExerciseCategoryUserMadeExercise");

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

            migrationBuilder.AddColumn<int>(
                name: "ExerciseBaseId",
                table: "ExerciseCategories",
                type: "integer",
                nullable: true);

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

            migrationBuilder.AddColumn<int>(
                name: "ExerciseCategoryId",
                table: "ExerciseBase",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserMadeExercise_ExerciseCategoryId",
                table: "ExerciseBase",
                type: "integer",
                nullable: true);

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
                name: "TrainingPlanCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TrainingPlanBaseId = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingPlanCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingPlanCategories_TrainingPlanBase_TrainingPlanBaseId",
                        column: x => x.TrainingPlanBaseId,
                        principalTable: "TrainingPlanBase",
                        principalColumn: "Id");
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
                name: "IX_ExerciseCategories_ExerciseBaseId",
                table: "ExerciseCategories",
                column: "ExerciseBaseId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseBase_ExerciseCategoryId",
                table: "ExerciseBase",
                column: "ExerciseCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseBase_UserMadeExercise_ExerciseCategoryId",
                table: "ExerciseBase",
                column: "UserMadeExercise_ExerciseCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityBase_ExerciseId",
                table: "ActivityBase",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityBase_PlanId",
                table: "ActivityBase",
                column: "PlanId");

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
                name: "IX_TrainingPlanCategories_TrainingPlanBaseId",
                table: "TrainingPlanCategories",
                column: "TrainingPlanBaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseBase_AspNetUsers_UserId",
                table: "ExerciseBase",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseBase_ExerciseCategories_ExerciseCategoryId",
                table: "ExerciseBase",
                column: "ExerciseCategoryId",
                principalTable: "ExerciseCategories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseBase_ExerciseCategories_UserMadeExercise_ExerciseCa~",
                table: "ExerciseBase",
                column: "UserMadeExercise_ExerciseCategoryId",
                principalTable: "ExerciseCategories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseCategories_ExerciseBase_ExerciseBaseId",
                table: "ExerciseCategories",
                column: "ExerciseBaseId",
                principalTable: "ExerciseBase",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseBase_AspNetUsers_UserId",
                table: "ExerciseBase");

            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseBase_ExerciseCategories_ExerciseCategoryId",
                table: "ExerciseBase");

            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseBase_ExerciseCategories_UserMadeExercise_ExerciseCa~",
                table: "ExerciseBase");

            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseCategories_ExerciseBase_ExerciseBaseId",
                table: "ExerciseCategories");

            migrationBuilder.DropTable(
                name: "ExerciseSets");

            migrationBuilder.DropTable(
                name: "TrainingPlanCategories");

            migrationBuilder.DropTable(
                name: "ActivityBase");

            migrationBuilder.DropTable(
                name: "TrainingPlanBase");

            migrationBuilder.DropIndex(
                name: "IX_ExerciseCategories_ExerciseBaseId",
                table: "ExerciseCategories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExerciseBase",
                table: "ExerciseBase");

            migrationBuilder.DropIndex(
                name: "IX_ExerciseBase_ExerciseCategoryId",
                table: "ExerciseBase");

            migrationBuilder.DropIndex(
                name: "IX_ExerciseBase_UserMadeExercise_ExerciseCategoryId",
                table: "ExerciseBase");

            migrationBuilder.DropColumn(
                name: "ExerciseBaseId",
                table: "ExerciseCategories");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "ExerciseBase");

            migrationBuilder.DropColumn(
                name: "ExerciseCategoryId",
                table: "ExerciseBase");

            migrationBuilder.DropColumn(
                name: "UserMadeExercise_ExerciseCategoryId",
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
                name: "IX_ExerciseCategoryUserMadeExercise_UserMadeExercisesId",
                table: "ExerciseCategoryUserMadeExercise",
                column: "UserMadeExercisesId");

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
