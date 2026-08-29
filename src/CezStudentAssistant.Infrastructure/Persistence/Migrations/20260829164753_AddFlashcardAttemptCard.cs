using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CezStudentAssistant.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFlashcardAttemptCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LearningCount",
                table: "FlashcardAttempts");

            migrationBuilder.DropColumn(
                name: "MasteredCount",
                table: "FlashcardAttempts");

            migrationBuilder.DropColumn(
                name: "ProgressPercentage",
                table: "FlashcardAttempts");

            migrationBuilder.CreateTable(
                name: "FlashcardAttemptCards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FlashcardAttemptId = table.Column<Guid>(type: "uuid", nullable: false),
                    FlashcardId = table.Column<Guid>(type: "uuid", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlashcardAttemptCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlashcardAttemptCards_FlashcardAttempts_FlashcardAttemptId",
                        column: x => x.FlashcardAttemptId,
                        principalTable: "FlashcardAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FlashcardAttemptCards_Flashcards_FlashcardId",
                        column: x => x.FlashcardId,
                        principalTable: "Flashcards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FlashcardAttemptCards_FlashcardAttemptId",
                table: "FlashcardAttemptCards",
                column: "FlashcardAttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_FlashcardAttemptCards_FlashcardId",
                table: "FlashcardAttemptCards",
                column: "FlashcardId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FlashcardAttemptCards");

            migrationBuilder.AddColumn<int>(
                name: "LearningCount",
                table: "FlashcardAttempts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MasteredCount",
                table: "FlashcardAttempts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProgressPercentage",
                table: "FlashcardAttempts",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
