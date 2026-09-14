using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagement.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddPlannedDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "Projects",
                newName: "PlannedStartDate");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "Projects",
                newName: "PlannedEndDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedEndDate",
                table: "ProjectSubTasks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedStartDate",
                table: "ProjectSubTasks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedEndDate",
                table: "GeneralRequirements",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedStartDate",
                table: "GeneralRequirements",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlannedEndDate",
                table: "ProjectSubTasks");

            migrationBuilder.DropColumn(
                name: "PlannedStartDate",
                table: "ProjectSubTasks");

            migrationBuilder.DropColumn(
                name: "PlannedEndDate",
                table: "GeneralRequirements");

            migrationBuilder.DropColumn(
                name: "PlannedStartDate",
                table: "GeneralRequirements");

            migrationBuilder.RenameColumn(
                name: "PlannedStartDate",
                table: "Projects",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "PlannedEndDate",
                table: "Projects",
                newName: "EndDate");
        }
    }
}
