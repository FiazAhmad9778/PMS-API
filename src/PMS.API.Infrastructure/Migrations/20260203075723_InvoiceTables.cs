using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PMS.API.Infrastructure.Migrations;

/// <inheritdoc />
public partial class InvoiceTables : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    // PostgreSQL requires USING for varchar -> bigint. Handle NULLs and non-numeric strings.
    migrationBuilder.Sql(@"
      ALTER TABLE ""Ward"" ALTER COLUMN ""externalId"" TYPE bigint USING (
        CASE WHEN ""externalId"" ~ '^-?[0-9]+$' THEN ""externalId""::bigint ELSE 0 END
      );
      ALTER TABLE ""Ward"" ALTER COLUMN ""externalId"" SET NOT NULL;
      ALTER TABLE ""Ward"" ALTER COLUMN ""externalId"" SET DEFAULT 0;
    ");

    migrationBuilder.Sql(@"
      ALTER TABLE ""Patient"" ALTER COLUMN ""patientId"" TYPE bigint USING (
        CASE WHEN ""patientId"" ~ '^-?[0-9]+$' THEN ""patientId""::bigint ELSE 0 END
      );
      ALTER TABLE ""Patient"" ALTER COLUMN ""patientId"" SET NOT NULL;
      ALTER TABLE ""Patient"" ALTER COLUMN ""patientId"" SET DEFAULT 0;
    ");

    migrationBuilder.AddColumn<long>(
        name: "organizationexternalid",
        table: "Organization",
        type: "bigint",
        nullable: false,
        defaultValue: 0L);

    migrationBuilder.CreateTable(
        name: "InvoiceHistory",
        columns: table => new
        {
          Id = table.Column<long>(type: "bigint", nullable: false)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
          OrganizationId = table.Column<long>(type: "bigint", nullable: true),
          PatientId = table.Column<long>(type: "bigint", nullable: true),
          IsSent = table.Column<bool>(type: "boolean", nullable: false),
          InvoiceStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
          InvoiceStatusHistory = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
          InvoiceStartDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
          InvoiceEndDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
          FilePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
          CreatedBy = table.Column<long>(type: "bigint", nullable: true),
          CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
          ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
          ModifiedBy = table.Column<long>(type: "bigint", nullable: true),
          IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
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
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
          InvoiceHistoryId = table.Column<long>(type: "bigint", nullable: false),
          WardId = table.Column<long>(type: "bigint", nullable: false),
          PatientIds = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
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
        name: "IX_InvoiceHistory_OrganizationId_InvoiceStartDate_InvoiceEndDa~",
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

    migrationBuilder.DropColumn(
        name: "organizationexternalid",
        table: "Organization");

    migrationBuilder.AlterColumn<string>(
        name: "externalId",
        table: "Ward",
        type: "character varying(100)",
        maxLength: 100,
        nullable: true,
        oldClrType: typeof(long),
        oldType: "bigint");

    migrationBuilder.AlterColumn<string>(
        name: "patientId",
        table: "Patient",
        type: "character varying(100)",
        maxLength: 100,
        nullable: true,
        oldClrType: typeof(long),
        oldType: "bigint");
  }
}
