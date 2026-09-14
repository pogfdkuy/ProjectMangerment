using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagement.Web.Migrations
{
    /// <inheritdoc />
    public partial class DifficultyBeforeAfter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DifficultyLevel",
                table: "Projects",
                newName: "DifficultyBefore");

            migrationBuilder.RenameColumn(
                name: "DifficultyLevel",
                table: "GeneralRequirements",
                newName: "DifficultyBefore");

            migrationBuilder.AddColumn<int>(
                name: "DifficultyAfter",
                table: "Projects",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DifficultyAfter",
                table: "GeneralRequirements",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DifficultyAfter",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "DifficultyAfter",
                table: "GeneralRequirements");

            migrationBuilder.RenameColumn(
                name: "DifficultyBefore",
                table: "Projects",
                newName: "DifficultyLevel");

            migrationBuilder.RenameColumn(
                name: "DifficultyBefore",
                table: "GeneralRequirements",
                newName: "DifficultyLevel");
        }
    }
}
