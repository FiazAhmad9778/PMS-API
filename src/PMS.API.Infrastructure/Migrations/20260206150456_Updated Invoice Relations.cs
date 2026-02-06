using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.API.Infrastructure.Migrations;

  /// <inheritdoc />
  public partial class UpdatedInvoiceRelations : Migration
  {
      /// <inheritdoc />
      protected override void Up(MigrationBuilder migrationBuilder)
      {
          migrationBuilder.CreateIndex(
              name: "IX_InvoiceHistory_PatientId",
              table: "InvoiceHistory",
              column: "PatientId");

          migrationBuilder.AddForeignKey(
              name: "FK_InvoiceHistory_Organization_OrganizationId",
              table: "InvoiceHistory",
              column: "OrganizationId",
              principalTable: "Organization",
              principalColumn: "id",
              onDelete: ReferentialAction.Cascade);

          migrationBuilder.AddForeignKey(
              name: "FK_InvoiceHistory_Patient_PatientId",
              table: "InvoiceHistory",
              column: "PatientId",
              principalTable: "Patient",
              principalColumn: "id",
              onDelete: ReferentialAction.Cascade);
      }

      /// <inheritdoc />
      protected override void Down(MigrationBuilder migrationBuilder)
      {
          migrationBuilder.DropForeignKey(
              name: "FK_InvoiceHistory_Organization_OrganizationId",
              table: "InvoiceHistory");

          migrationBuilder.DropForeignKey(
              name: "FK_InvoiceHistory_Patient_PatientId",
              table: "InvoiceHistory");

          migrationBuilder.DropIndex(
              name: "IX_InvoiceHistory_PatientId",
              table: "InvoiceHistory");
      }
  }
