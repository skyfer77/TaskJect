using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace Data.Migrations
{
    public partial class UpdatePrjectAddRowGitHub : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "GitHubInstallationId",
                table: "Project",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GitHubOwner",
                table: "Project",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GitHubRepoName",
                table: "Project",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GitHubInstallationId",
                table: "Project");

            migrationBuilder.DropColumn(
                name: "GitHubOwner",
                table: "Project");

            migrationBuilder.DropColumn(
                name: "GitHubRepoName",
                table: "Project");
        }
    }
}
