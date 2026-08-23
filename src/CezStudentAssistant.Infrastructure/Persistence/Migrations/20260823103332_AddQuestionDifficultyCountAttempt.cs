using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CezStudentAssistant.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionDifficultyCountAttempt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EasyQuestionCountPerAttempt",
                table: "Quiz",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HardQuestionCountPerAttempt",
                table: "Quiz",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MediumQuestionCountPerAttempt",
                table: "Quiz",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EasyQuestionCountPerAttempt",
                table: "Quiz");

            migrationBuilder.DropColumn(
                name: "HardQuestionCountPerAttempt",
                table: "Quiz");

            migrationBuilder.DropColumn(
                name: "MediumQuestionCountPerAttempt",
                table: "Quiz");
        }
    }
}
