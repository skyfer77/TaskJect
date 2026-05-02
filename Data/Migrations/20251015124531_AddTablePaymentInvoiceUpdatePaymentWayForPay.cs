using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTablePaymentInvoiceUpdatePaymentWayForPay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaidAt",
                table: "PaymentWayForPay");

            migrationBuilder.RenameColumn(
                name: "WayForPayResponse",
                table: "PaymentWayForPay",
                newName: "RecToken");

            migrationBuilder.CreateTable(
                name: "PaymentInvoice",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentWayForPayId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WayForPayResponse = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentInvoice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentInvoice_PaymentWayForPay_PaymentWayForPayId",
                        column: x => x.PaymentWayForPayId,
                        principalTable: "PaymentWayForPay",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentInvoice_PaymentWayForPayId",
                table: "PaymentInvoice",
                column: "PaymentWayForPayId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentInvoice");

            migrationBuilder.RenameColumn(
                name: "RecToken",
                table: "PaymentWayForPay",
                newName: "WayForPayResponse");

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidAt",
                table: "PaymentWayForPay",
                type: "datetime2",
                nullable: true);
        }
    }
}
