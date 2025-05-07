using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClaudeCodeCurator.Migrations
{
    /// <inheritdoc />
    public partial class ChangeReferenceUserStoryToBoolean2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First add a temporary column with boolean type
            migrationBuilder.AddColumn<bool>(
                name: "ReferenceUserStoryBool",
                table: "Tasks",
                type: "boolean",
                nullable: false,
                defaultValue: false);
                
            // Update the temporary column based on the original string column
            // 'true' string converts to true boolean
            // any other value converts to false
            migrationBuilder.Sql(@"
                UPDATE ""Tasks"" 
                SET ""ReferenceUserStoryBool"" = CASE 
                    WHEN ""ReferenceUserStory"" = 'true' THEN true 
                    ELSE false 
                END;
            ");
            
            // Drop the original string column
            migrationBuilder.DropColumn(
                name: "ReferenceUserStory",
                table: "Tasks");
                
            // Rename the temporary column to the original name
            migrationBuilder.RenameColumn(
                name: "ReferenceUserStoryBool",
                table: "Tasks",
                newName: "ReferenceUserStory");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // First add a temporary column with string type
            migrationBuilder.AddColumn<string>(
                name: "ReferenceUserStoryString",
                table: "Tasks",
                type: "text",
                nullable: false,
                defaultValue: "");
                
            // Update the temporary column based on the boolean column
            migrationBuilder.Sql(@"
                UPDATE ""Tasks"" 
                SET ""ReferenceUserStoryString"" = CASE 
                    WHEN ""ReferenceUserStory"" = true THEN 'true' 
                    ELSE '' 
                END;
            ");
            
            // Drop the boolean column
            migrationBuilder.DropColumn(
                name: "ReferenceUserStory",
                table: "Tasks");
                
            // Rename the temporary column to the original name
            migrationBuilder.RenameColumn(
                name: "ReferenceUserStoryString",
                table: "Tasks",
                newName: "ReferenceUserStory");
        }
    }
}
