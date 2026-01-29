using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.API.Infrastructure.Migrations;

  /// <inheritdoc />
  public partial class InvoiceHistoryTableUpdated : Migration
  {
      /// <inheritdoc />
      protected override void Up(MigrationBuilder migrationBuilder)
      {
          migrationBuilder.RenameColumn(
              name: "PatientInvoiceHistoryWardId",
              table: "PatientInvoiceHistoryWard",
              newName: "Id");

          migrationBuilder.RenameColumn(
              name: "PatientInvoiceHistoryId",
              table: "PatientInvoiceHistory",
              newName: "Id");

          migrationBuilder.AlterColumn<long>(
              name: "externalId",
              table: "Ward",
              type: "bigint",
              maxLength: 100,
              nullable: false,
              defaultValue: 0L,
              oldClrType: typeof(string),
              oldType: "nvarchar(100)",
              oldMaxLength: 100,
              oldNullable: true);

          migrationBuilder.AlterColumn<string>(
              name: "PatientIds",
              table: "PatientInvoiceHistoryWard",
              type: "nvarchar(2000)",
              maxLength: 2000,
              nullable: true,
              oldClrType: typeof(string),
              oldType: "nvarchar(max)",
              oldNullable: true);

          migrationBuilder.AlterColumn<bool>(
              name: "IsDeleted",
              table: "PatientInvoiceHistory",
              type: "bit",
              nullable: false,
              defaultValue: false,
              oldClrType: typeof(bool),
              oldType: "bit");

          migrationBuilder.AlterColumn<string>(
              name: "InvoiceSendingWays",
              table: "PatientInvoiceHistory",
              type: "nvarchar(200)",
              maxLength: 200,
              nullable: true,
              oldClrType: typeof(string),
              oldType: "nvarchar(max)",
              oldNullable: true);

          migrationBuilder.AlterColumn<string>(
              name: "FilePath",
              table: "PatientInvoiceHistory",
              type: "nvarchar(1000)",
              maxLength: 1000,
              nullable: true,
              oldClrType: typeof(string),
              oldType: "nvarchar(max)",
              oldNullable: true);

          migrationBuilder.AddColumn<bool>(
              name: "IsSent",
              table: "PatientInvoiceHistory",
              type: "bit",
              nullable: false,
              defaultValue: false);

          migrationBuilder.AlterColumn<long>(
              name: "patientId",
              table: "Patient",
              type: "bigint",
              maxLength: 100,
              nullable: false,
              defaultValue: 0L,
              oldClrType: typeof(string),
              oldType: "nvarchar(100)",
              oldMaxLength: 100,
              oldNullable: true);

          migrationBuilder.CreateIndex(
              name: "IX_PatientInvoiceHistoryWard_WardId",
              table: "PatientInvoiceHistoryWard",
              column: "WardId");

          migrationBuilder.CreateIndex(
              name: "IX_PatientInvoiceHistory_OrganizationId_InvoiceStartDate_InvoiceEndDate",
              table: "PatientInvoiceHistory",
              columns: new[] { "OrganizationId", "InvoiceStartDate", "InvoiceEndDate" });
      }

      /// <inheritdoc />
      protected override void Down(MigrationBuilder migrationBuilder)
      {
          migrationBuilder.DropIndex(
              name: "IX_PatientInvoiceHistoryWard_WardId",
              table: "PatientInvoiceHistoryWard");

          migrationBuilder.DropIndex(
              name: "IX_PatientInvoiceHistory_OrganizationId_InvoiceStartDate_InvoiceEndDate",
              table: "PatientInvoiceHistory");

          migrationBuilder.DropColumn(
              name: "IsSent",
              table: "PatientInvoiceHistory");

          migrationBuilder.RenameColumn(
              name: "Id",
              table: "PatientInvoiceHistoryWard",
              newName: "PatientInvoiceHistoryWardId");

          migrationBuilder.RenameColumn(
              name: "Id",
              table: "PatientInvoiceHistory",
              newName: "PatientInvoiceHistoryId");

          migrationBuilder.AlterColumn<string>(
              name: "externalId",
              table: "Ward",
              type: "nvarchar(100)",
              maxLength: 100,
              nullable: true,
              oldClrType: typeof(long),
              oldType: "bigint",
              oldMaxLength: 100);

          migrationBuilder.AlterColumn<string>(
              name: "PatientIds",
              table: "PatientInvoiceHistoryWard",
              type: "nvarchar(max)",
              nullable: true,
              oldClrType: typeof(string),
              oldType: "nvarchar(2000)",
              oldMaxLength: 2000,
              oldNullable: true);

          migrationBuilder.AlterColumn<bool>(
              name: "IsDeleted",
              table: "PatientInvoiceHistory",
              type: "bit",
              nullable: false,
              oldClrType: typeof(bool),
              oldType: "bit",
              oldDefaultValue: false);

          migrationBuilder.AlterColumn<string>(
              name: "InvoiceSendingWays",
              table: "PatientInvoiceHistory",
              type: "nvarchar(max)",
              nullable: true,
              oldClrType: typeof(string),
              oldType: "nvarchar(200)",
              oldMaxLength: 200,
              oldNullable: true);

          migrationBuilder.AlterColumn<string>(
              name: "FilePath",
              table: "PatientInvoiceHistory",
              type: "nvarchar(max)",
              nullable: true,
              oldClrType: typeof(string),
              oldType: "nvarchar(1000)",
              oldMaxLength: 1000,
              oldNullable: true);

          migrationBuilder.AlterColumn<string>(
              name: "patientId",
              table: "Patient",
              type: "nvarchar(100)",
              maxLength: 100,
              nullable: true,
              oldClrType: typeof(long),
              oldType: "bigint",
              oldMaxLength: 100);
      }
  }
