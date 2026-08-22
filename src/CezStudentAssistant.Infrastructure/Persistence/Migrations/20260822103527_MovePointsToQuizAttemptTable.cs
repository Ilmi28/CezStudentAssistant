using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CezStudentAssistant.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MovePointsToQuizAttemptTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EarnedPoints",
                table: "QuestionAnswers");

            migrationBuilder.AddColumn<decimal>(
                name: "Points",
                table: "QuizAttempts",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Points",
                table: "QuizAttempts");

            migrationBuilder.AddColumn<decimal>(
                name: "EarnedPoints",
                table: "QuestionAnswers",
                type: "numeric",
                nullable: true);
        }
    }
}
