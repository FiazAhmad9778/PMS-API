using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.API.Infrastructure.Migrations;

  /// <inheritdoc />
  public partial class AddExternalIdinORG : Migration
  {
      /// <inheritdoc />
      protected override void Up(MigrationBuilder migrationBuilder)
      {
          migrationBuilder.AddColumn<long>(
              name: "OrganizationExternalId",
              table: "Organization",
              type: "bigint",
              nullable: false,
              defaultValue: 0L);
      }

      /// <inheritdoc />
      protected override void Down(MigrationBuilder migrationBuilder)
      {
          migrationBuilder.DropColumn(
              name: "OrganizationExternalId",
              table: "Organization");
      }
  }
