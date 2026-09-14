using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagement.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemNameNotesTicketNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationTicketNumber",
                table: "Projects",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DifficultyAfterNote",
                table: "Projects",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DifficultyBeforeNote",
                table: "Projects",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HasCrossUnitBenefitNote",
                table: "Projects",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HasLongTermBenefitNote",
                table: "Projects",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HasPrimaryBenefitNote",
                table: "Projects",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SystemName",
                table: "Projects",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicationTicketNumber",
                table: "GeneralRequirements",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DifficultyAfterNote",
                table: "GeneralRequirements",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DifficultyBeforeNote",
                table: "GeneralRequirements",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HasCrossUnitBenefitNote",
                table: "GeneralRequirements",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HasLongTermBenefitNote",
                table: "GeneralRequirements",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HasPrimaryBenefitNote",
                table: "GeneralRequirements",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SystemName",
                table: "GeneralRequirements",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicationTicketNumber",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "DifficultyAfterNote",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "DifficultyBeforeNote",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "HasCrossUnitBenefitNote",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "HasLongTermBenefitNote",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "HasPrimaryBenefitNote",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "SystemName",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ApplicationTicketNumber",
                table: "GeneralRequirements");

            migrationBuilder.DropColumn(
                name: "DifficultyAfterNote",
                table: "GeneralRequirements");

            migrationBuilder.DropColumn(
                name: "DifficultyBeforeNote",
                table: "GeneralRequirements");

            migrationBuilder.DropColumn(
                name: "HasCrossUnitBenefitNote",
                table: "GeneralRequirements");

            migrationBuilder.DropColumn(
                name: "HasLongTermBenefitNote",
                table: "GeneralRequirements");

            migrationBuilder.DropColumn(
                name: "HasPrimaryBenefitNote",
                table: "GeneralRequirements");

            migrationBuilder.DropColumn(
                name: "SystemName",
                table: "GeneralRequirements");
        }
    }
}
