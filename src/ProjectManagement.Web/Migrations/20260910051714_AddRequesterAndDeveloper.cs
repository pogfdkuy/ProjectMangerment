using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagement.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddRequesterAndDeveloper : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeveloperUserId",
                table: "Projects",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequesterUserId",
                table: "Projects",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeveloperUserId",
                table: "GeneralRequirements",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequesterUserId",
                table: "GeneralRequirements",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_DeveloperUserId",
                table: "Projects",
                column: "DeveloperUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_RequesterUserId",
                table: "Projects",
                column: "RequesterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralRequirements_DeveloperUserId",
                table: "GeneralRequirements",
                column: "DeveloperUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralRequirements_RequesterUserId",
                table: "GeneralRequirements",
                column: "RequesterUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GeneralRequirements_AspNetUsers_DeveloperUserId",
                table: "GeneralRequirements",
                column: "DeveloperUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GeneralRequirements_AspNetUsers_RequesterUserId",
                table: "GeneralRequirements",
                column: "RequesterUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_AspNetUsers_DeveloperUserId",
                table: "Projects",
                column: "DeveloperUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_AspNetUsers_RequesterUserId",
                table: "Projects",
                column: "RequesterUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GeneralRequirements_AspNetUsers_DeveloperUserId",
                table: "GeneralRequirements");

            migrationBuilder.DropForeignKey(
                name: "FK_GeneralRequirements_AspNetUsers_RequesterUserId",
                table: "GeneralRequirements");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_AspNetUsers_DeveloperUserId",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_AspNetUsers_RequesterUserId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_DeveloperUserId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_RequesterUserId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_GeneralRequirements_DeveloperUserId",
                table: "GeneralRequirements");

            migrationBuilder.DropIndex(
                name: "IX_GeneralRequirements_RequesterUserId",
                table: "GeneralRequirements");

            migrationBuilder.DropColumn(
                name: "DeveloperUserId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "RequesterUserId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "DeveloperUserId",
                table: "GeneralRequirements");

            migrationBuilder.DropColumn(
                name: "RequesterUserId",
                table: "GeneralRequirements");
        }
    }
}
