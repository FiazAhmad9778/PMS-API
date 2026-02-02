using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.API.Infrastructure.Migrations;

  /// <inheritdoc />
  public partial class AddStatusinInvoiceHistory : Migration
  {
      /// <inheritdoc />
      protected override void Up(MigrationBuilder migrationBuilder)
      {
          migrationBuilder.AddColumn<string>(
              name: "InvoiceStatus",
              table: "PatientInvoiceHistory",
              type: "nvarchar(max)",
              nullable: true);

          migrationBuilder.AddColumn<long>(
              name: "PatientId",
              table: "PatientInvoiceHistory",
              type: "bigint",
              nullable: false,
              defaultValue: 0L);
      }

      /// <inheritdoc />
      protected override void Down(MigrationBuilder migrationBuilder)
      {
          migrationBuilder.DropColumn(
              name: "InvoiceStatus",
              table: "PatientInvoiceHistory");

          migrationBuilder.DropColumn(
              name: "PatientId",
              table: "PatientInvoiceHistory");
      }
  }
