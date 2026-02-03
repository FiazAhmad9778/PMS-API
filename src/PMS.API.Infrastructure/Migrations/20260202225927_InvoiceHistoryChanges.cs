using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.API.Infrastructure.Migrations;

/// <inheritdoc />
public partial class InvoiceHistoryChanges : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropTable(
        name: "PatientInvoiceHistoryWard");

    migrationBuilder.DropTable(
        name: "PatientInvoiceHistory");

    migrationBuilder.CreateTable(
        name: "InvoiceHistory",
        columns: table => new
        {
          Id = table.Column<long>(type: "bigint", nullable: false)
                .Annotation("SqlServer:Identity", "1, 1"),
          OrganizationId = table.Column<long>(type: "bigint", nullable: true),
          PatientId = table.Column<long>(type: "bigint", nullable: true),
          IsSent = table.Column<bool>(type: "bit", nullable: false),
          InvoiceStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
          InvoiceStatusHistory = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
          InvoiceStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
          InvoiceEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
          FilePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
          CreatedBy = table.Column<long>(type: "bigint", nullable: true),
          CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
          ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
          ModifiedBy = table.Column<long>(type: "bigint", nullable: true),
          IsDeleted = table.Column<bool>(type: "bit", nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_InvoiceHistory", x => x.Id);
        });

    migrationBuilder.CreateTable(
        name: "InvoiceHistoryWard",
        columns: table => new
        {
          Id = table.Column<long>(type: "bigint", nullable: false)
                .Annotation("SqlServer:Identity", "1, 1"),
          InvoiceHistoryId = table.Column<long>(type: "bigint", nullable: false),
          WardId = table.Column<long>(type: "bigint", nullable: false),
          PatientIds = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_InvoiceHistoryWard", x => x.Id);
          table.ForeignKey(
                    name: "FK_InvoiceHistoryWard_InvoiceHistory_InvoiceHistoryId",
                    column: x => x.InvoiceHistoryId,
                    principalTable: "InvoiceHistory",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
        });

    migrationBuilder.CreateIndex(
        name: "IX_InvoiceHistory_InvoiceStatus",
        table: "InvoiceHistory",
        column: "InvoiceStatus");

    migrationBuilder.CreateIndex(
        name: "IX_InvoiceHistory_OrganizationId_InvoiceStartDate_InvoiceEndDate",
        table: "InvoiceHistory",
        columns: new[] { "OrganizationId", "InvoiceStartDate", "InvoiceEndDate" });

    migrationBuilder.CreateIndex(
        name: "IX_InvoiceHistoryWard_InvoiceHistoryId",
        table: "InvoiceHistoryWard",
        column: "InvoiceHistoryId");

    migrationBuilder.CreateIndex(
        name: "IX_InvoiceHistoryWard_WardId",
        table: "InvoiceHistoryWard",
        column: "WardId");
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropTable(
        name: "InvoiceHistoryWard");

    migrationBuilder.DropTable(
        name: "InvoiceHistory");

    migrationBuilder.CreateTable(
        name: "PatientInvoiceHistory",
        columns: table => new
        {
          Id = table.Column<long>(type: "bigint", nullable: false)
                .Annotation("SqlServer:Identity", "1, 1"),
          CreatedBy = table.Column<long>(type: "bigint", nullable: true),
          CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
          FilePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
          InvoiceEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
          InvoiceSendingWays = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
          InvoiceStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
          InvoiceStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
          IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
          IsSent = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
          ModifiedBy = table.Column<long>(type: "bigint", nullable: true),
          ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
          OrganizationId = table.Column<long>(type: "bigint", nullable: false),
          PatientId = table.Column<long>(type: "bigint", nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_PatientInvoiceHistory", x => x.Id);
        });

    migrationBuilder.CreateTable(
        name: "PatientInvoiceHistoryWard",
        columns: table => new
        {
          Id = table.Column<long>(type: "bigint", nullable: false)
                .Annotation("SqlServer:Identity", "1, 1"),
          PatientInvoiceHistoryId = table.Column<long>(type: "bigint", nullable: false),
          PatientIds = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
          WardId = table.Column<long>(type: "bigint", nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_PatientInvoiceHistoryWard", x => x.Id);
          table.ForeignKey(
                    name: "FK_PatientInvoiceHistoryWard_PatientInvoiceHistory_PatientInvoiceHistoryId",
                    column: x => x.PatientInvoiceHistoryId,
                    principalTable: "PatientInvoiceHistory",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
        });

    migrationBuilder.CreateIndex(
        name: "IX_PatientInvoiceHistory_OrganizationId_InvoiceStartDate_InvoiceEndDate",
        table: "PatientInvoiceHistory",
        columns: new[] { "OrganizationId", "InvoiceStartDate", "InvoiceEndDate" });

    migrationBuilder.CreateIndex(
        name: "IX_PatientInvoiceHistoryWard_PatientInvoiceHistoryId",
        table: "PatientInvoiceHistoryWard",
        column: "PatientInvoiceHistoryId");

    migrationBuilder.CreateIndex(
        name: "IX_PatientInvoiceHistoryWard_WardId",
        table: "PatientInvoiceHistoryWard",
        column: "WardId");
  }
}
