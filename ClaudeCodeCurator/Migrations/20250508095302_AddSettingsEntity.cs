using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClaudeCodeCurator.Migrations
{
    /// <inheritdoc />
    public partial class AddSettingsEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    OpenAiApiKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    GoogleAiApiKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AnthropicAiApiKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OpenAiModel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    GoogleAiModel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AnthropicAiModel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Settings");
        }
    }
}
