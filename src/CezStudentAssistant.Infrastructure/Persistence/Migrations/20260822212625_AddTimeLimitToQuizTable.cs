using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CezStudentAssistant.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeLimitToQuizTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TimeLimitMinutes",
                table: "Quiz",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeLimitMinutes",
                table: "Quiz");
        }
    }
}
