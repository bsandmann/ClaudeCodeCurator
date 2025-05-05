using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClaudeCodeCurator.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UserStoryNumberCounter = table.Column<int>(type: "integer", nullable: false),
                    TaskNumberCounter = table.Column<int>(type: "integer", nullable: false),
                    CreatedOrUpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserStories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    UserStoryNumber = table.Column<int>(type: "integer", nullable: false),
                    CreatedOrUpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserStories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserStories_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PromptBody = table.Column<string>(type: "text", nullable: false),
                    TaskNumber = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ApprovedByUserUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RequestedByAiUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FinishedByAiUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Paused = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOrUpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserStoryId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tasks_UserStories_UserStoryId",
                        column: x => x.UserStoryId,
                        principalTable: "UserStories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectTaskOrders",
                columns: table => new
                {
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectTaskOrders", x => new { x.ProjectId, x.TaskId });
                    table.ForeignKey(
                        name: "FK_ProjectTaskOrders_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectTaskOrders_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTaskOrders_ProjectId_Position",
                table: "ProjectTaskOrders",
                columns: new[] { "ProjectId", "Position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTaskOrders_TaskId",
                table: "ProjectTaskOrders",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_UserStoryId",
                table: "Tasks",
                column: "UserStoryId");

            migrationBuilder.CreateIndex(
                name: "IX_UserStories_ProjectId",
                table: "UserStories",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectTaskOrders");

            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.DropTable(
                name: "UserStories");

            migrationBuilder.DropTable(
                name: "Projects");
        }
    }
}
