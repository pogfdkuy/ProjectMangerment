using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagement.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddManualPersonNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeveloperUserName",
                table: "Projects",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequesterUserName",
                table: "Projects",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsibleUserName",
                table: "Projects",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeveloperUserName",
                table: "GeneralRequirements",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequesterUserName",
                table: "GeneralRequirements",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsibleUserName",
                table: "GeneralRequirements",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeveloperUserName",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "RequesterUserName",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ResponsibleUserName",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "DeveloperUserName",
                table: "GeneralRequirements");

            migrationBuilder.DropColumn(
                name: "RequesterUserName",
                table: "GeneralRequirements");

            migrationBuilder.DropColumn(
                name: "ResponsibleUserName",
                table: "GeneralRequirements");
        }
    }
}
