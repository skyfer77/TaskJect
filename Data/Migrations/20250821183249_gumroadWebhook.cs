using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class gumroadWebhook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GumroadWebhookLog",
                columns: table => new
                {
                    EventId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProcessedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventType = table.Column<int>(type: "int", nullable: false),
                    SaleId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SubscriptionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OrganizationCode = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GumroadWebhookLog", x => x.EventId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GumroadWebhookLog_EventType",
                table: "GumroadWebhookLog",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_GumroadWebhookLog_SaleId",
                table: "GumroadWebhookLog",
                column: "SaleId");

            migrationBuilder.CreateIndex(
                name: "IX_GumroadWebhookLog_SubscriptionId",
                table: "GumroadWebhookLog",
                column: "SubscriptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GumroadWebhookLog");
        }
    }
}
