using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscountColumnInTariffPlanTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PriceWithDiscount",
                table: "TariffPlan",
                newName: "PriceYearlyDiscount");

            migrationBuilder.AddColumn<string>(
                name: "PriceMonthlyDiscount",
                table: "TariffPlan",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PriceMonthlyDiscount",
                table: "TariffPlan");

            migrationBuilder.RenameColumn(
                name: "PriceYearlyDiscount",
                table: "TariffPlan",
                newName: "PriceWithDiscount");
        }
    }
}
