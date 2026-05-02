using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedFullInfoToTariffPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasPriorityAppealsProcess",
                table: "TariffPlan",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasProjectAccessControl",
                table: "TariffPlan",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PriceMonth",
                table: "TariffPlan",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PriceWithDiscount",
                table: "TariffPlan",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasPriorityAppealsProcess",
                table: "TariffPlan");

            migrationBuilder.DropColumn(
                name: "HasProjectAccessControl",
                table: "TariffPlan");

            migrationBuilder.DropColumn(
                name: "PriceMonth",
                table: "TariffPlan");

            migrationBuilder.DropColumn(
                name: "PriceWithDiscount",
                table: "TariffPlan");
        }
    }
}
