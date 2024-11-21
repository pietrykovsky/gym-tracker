using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GymTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainingActivityAndSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Discriminator",
                table: "ActivityBase",
                type: "character varying(21)",
                maxLength: 21,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(13)",
                oldMaxLength: 13);

            migrationBuilder.AddColumn<int>(
                name: "TrainingSessionId",
                table: "ActivityBase",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TrainingSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingSessions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityBase_TrainingSessionId",
                table: "ActivityBase",
                column: "TrainingSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingSessions_UserId",
                table: "TrainingSessions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityBase_TrainingSessions_TrainingSessionId",
                table: "ActivityBase",
                column: "TrainingSessionId",
                principalTable: "TrainingSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivityBase_TrainingSessions_TrainingSessionId",
                table: "ActivityBase");

            migrationBuilder.DropTable(
                name: "TrainingSessions");

            migrationBuilder.DropIndex(
                name: "IX_ActivityBase_TrainingSessionId",
                table: "ActivityBase");

            migrationBuilder.DropColumn(
                name: "TrainingSessionId",
                table: "ActivityBase");

            migrationBuilder.AlterColumn<string>(
                name: "Discriminator",
                table: "ActivityBase",
                type: "character varying(13)",
                maxLength: 13,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(21)",
                oldMaxLength: 21);
        }
    }
}
