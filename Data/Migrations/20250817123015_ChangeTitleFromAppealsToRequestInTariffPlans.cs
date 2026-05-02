using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTitleFromAppealsToRequestInTariffPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HasPriorityAppealsProcess",
                table: "TariffPlan",
                newName: "HasPriorityRequestsProcess");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HasPriorityRequestsProcess",
                table: "TariffPlan",
                newName: "HasPriorityAppealsProcess");
        }
    }
}
