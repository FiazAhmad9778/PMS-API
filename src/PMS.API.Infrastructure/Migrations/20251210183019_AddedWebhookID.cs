using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.API.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedWebhookID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "webhookId",
                table: "Order",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Order_webhookId",
                table: "Order",
                column: "webhookId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Order_webhookId",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "webhookId",
                table: "Order");
        }
    }
}
