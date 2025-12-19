using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PMS.API.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddedOrderTable : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.CreateTable(
        name: "Order",
        columns: table => new
        {
          id = table.Column<long>(type: "bigint", nullable: false)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
          firstName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
          lastName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
          phoneNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
          medication = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
          deliveryOrPickup = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
          address = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
          deliveryTimeSlot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
          notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
          faxStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, defaultValue: "Pending"),
          faxTransactionId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
          faxSentAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
          faxRetryCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
          faxErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
          createdDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
          createdBy = table.Column<long>(type: "bigint", nullable: true),
          modifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
          modifiedBy = table.Column<long>(type: "bigint", nullable: true),
          isDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Order", x => x.id);
        });

    migrationBuilder.CreateIndex(
        name: "IX_Order_createdDate",
        table: "Order",
        column: "createdDate");

    migrationBuilder.CreateIndex(
        name: "IX_Order_faxStatus",
        table: "Order",
        column: "faxStatus");

    migrationBuilder.CreateIndex(
        name: "IX_Order_faxStatus_createdDate",
        table: "Order",
        columns: new[] { "faxStatus", "createdDate" });
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropTable(
        name: "Order");
  }
}
