using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CezStudentAssistant.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFlashcardAttempt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CardCountPerAttempt",
                table: "FlashcardDecks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EasyCardCountPerAttempt",
                table: "FlashcardDecks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HardCardCountPerAttempt",
                table: "FlashcardDecks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MediumCardCountPerAttempt",
                table: "FlashcardDecks",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FlashcardAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeckId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CardCount = table.Column<int>(type: "integer", nullable: false),
                    MasteredCount = table.Column<int>(type: "integer", nullable: false),
                    LearningCount = table.Column<int>(type: "integer", nullable: false),
                    ProgressPercentage = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlashcardAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlashcardAttempts_FlashcardDecks_DeckId",
                        column: x => x.DeckId,
                        principalTable: "FlashcardDecks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FlashcardAttempts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FlashcardAttempts_DeckId",
                table: "FlashcardAttempts",
                column: "DeckId");

            migrationBuilder.CreateIndex(
                name: "IX_FlashcardAttempts_UserId",
                table: "FlashcardAttempts",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FlashcardAttempts");

            migrationBuilder.DropColumn(
                name: "CardCountPerAttempt",
                table: "FlashcardDecks");

            migrationBuilder.DropColumn(
                name: "EasyCardCountPerAttempt",
                table: "FlashcardDecks");

            migrationBuilder.DropColumn(
                name: "HardCardCountPerAttempt",
                table: "FlashcardDecks");

            migrationBuilder.DropColumn(
                name: "MediumCardCountPerAttempt",
                table: "FlashcardDecks");
        }
    }
}
