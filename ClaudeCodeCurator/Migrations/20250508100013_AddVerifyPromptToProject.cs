using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClaudeCodeCurator.Migrations
{
    /// <inheritdoc />
    public partial class AddVerifyPromptToProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VerifyPrompt",
                table: "Projects",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VerifyPrompt",
                table: "Projects");
        }
    }
}
