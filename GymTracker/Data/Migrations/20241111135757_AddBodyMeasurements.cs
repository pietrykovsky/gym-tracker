using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GymTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddBodyMeasurements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BodyMeasurements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Weight = table.Column<float>(type: "real", precision: 2, scale: 1, nullable: true),
                    Height = table.Column<float>(type: "real", precision: 2, scale: 1, nullable: true),
                    FatMassPercentage = table.Column<float>(type: "real", precision: 2, scale: 1, nullable: true),
                    MuscleMassPercentage = table.Column<float>(type: "real", precision: 2, scale: 1, nullable: true),
                    WaistCircumference = table.Column<float>(type: "real", precision: 2, scale: 1, nullable: true),
                    ChestCircumference = table.Column<float>(type: "real", precision: 2, scale: 1, nullable: true),
                    ArmCircumference = table.Column<float>(type: "real", precision: 2, scale: 1, nullable: true),
                    ThighCircumference = table.Column<float>(type: "real", precision: 2, scale: 1, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BodyMeasurements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BodyMeasurements_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BodyMeasurements_UserId",
                table: "BodyMeasurements",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BodyMeasurements");
        }
    }
}
