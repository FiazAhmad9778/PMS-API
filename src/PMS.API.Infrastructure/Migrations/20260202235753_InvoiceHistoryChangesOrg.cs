using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.API.Infrastructure.Migrations;

/// <inheritdoc />
public partial class InvoiceHistoryChangesOrg : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.RenameColumn(
        name: "OrganizationExternalId",
        table: "Organization",
        newName: "organizationexternalid");
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.RenameColumn(
        name: "organizationexternalid",
        table: "Organization",
        newName: "OrganizationExternalId");
  }
}
