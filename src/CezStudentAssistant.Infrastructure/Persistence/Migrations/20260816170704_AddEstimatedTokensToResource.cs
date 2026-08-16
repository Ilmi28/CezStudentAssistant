using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CezStudentAssistant.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEstimatedTokensToResource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EstimatedTokens",
                table: "Resources",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimatedTokens",
                table: "Resources");
        }
    }
}
