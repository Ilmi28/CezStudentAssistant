using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CezStudentAssistant.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedSnapshotInfoToQuizAttempt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MaxPoints",
                table: "QuizAttempts",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuestionCount",
                table: "QuizAttempts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TimeLimitMinutes",
                table: "QuizAttempts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuestionCountPerAttempt",
                table: "Quiz",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxPoints",
                table: "QuizAttempts");

            migrationBuilder.DropColumn(
                name: "QuestionCount",
                table: "QuizAttempts");

            migrationBuilder.DropColumn(
                name: "TimeLimitMinutes",
                table: "QuizAttempts");

            migrationBuilder.DropColumn(
                name: "QuestionCountPerAttempt",
                table: "Quiz");
        }
    }
}
