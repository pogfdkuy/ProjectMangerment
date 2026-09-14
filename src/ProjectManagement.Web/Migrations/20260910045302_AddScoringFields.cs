using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagement.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddScoringFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DifficultyLevel",
                table: "Projects",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasCrossUnitBenefit",
                table: "Projects",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasLongTermBenefit",
                table: "Projects",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasPrimaryBenefit",
                table: "Projects",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "DifficultyLevel",
                table: "GeneralRequirements",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasCrossUnitBenefit",
                table: "GeneralRequirements",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasLongTermBenefit",
                table: "GeneralRequirements",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasPrimaryBenefit",
                table: "GeneralRequirements",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DifficultyLevel",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "HasCrossUnitBenefit",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "HasLongTermBenefit",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "HasPrimaryBenefit",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "DifficultyLevel",
                table: "GeneralRequirements");

            migrationBuilder.DropColumn(
                name: "HasCrossUnitBenefit",
                table: "GeneralRequirements");

            migrationBuilder.DropColumn(
                name: "HasLongTermBenefit",
                table: "GeneralRequirements");

            migrationBuilder.DropColumn(
                name: "HasPrimaryBenefit",
                table: "GeneralRequirements");
        }
    }
}
