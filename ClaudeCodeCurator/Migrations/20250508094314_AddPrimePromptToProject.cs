using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClaudeCodeCurator.Migrations
{
    /// <inheritdoc />
    public partial class AddPrimePromptToProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PrimePrompt",
                table: "Projects",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrimePrompt",
                table: "Projects");
        }
    }
}
