using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagement.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddEstimatedHoursAndScheduleLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedHours",
                table: "GeneralRequirements",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ScheduleChangeLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerType = table.Column<int>(type: "int", nullable: false),
                    OwnerId = table.Column<int>(type: "int", nullable: false),
                    OldPlannedStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NewPlannedStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OldPlannedEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NewPlannedEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ChangedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleChangeLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleChangeLogs_AspNetUsers_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleChangeLogs_ChangedByUserId",
                table: "ScheduleChangeLogs",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleChangeLogs_OwnerType_OwnerId",
                table: "ScheduleChangeLogs",
                columns: new[] { "OwnerType", "OwnerId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduleChangeLogs");

            migrationBuilder.DropColumn(
                name: "EstimatedHours",
                table: "GeneralRequirements");
        }
    }
}
