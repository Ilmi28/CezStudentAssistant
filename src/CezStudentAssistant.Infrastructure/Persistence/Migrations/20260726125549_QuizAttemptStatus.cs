using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CezStudentAssistant.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class QuizAttemptStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "QuizAttempts");

            migrationBuilder.DropColumn(
                name: "Score",
                table: "QuizAttempts");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "QuizAttempts",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "QuizAttempts");

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "QuizAttempts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Score",
                table: "QuizAttempts",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
