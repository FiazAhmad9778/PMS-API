using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.API.Infrastructure.Migrations;

  /// <inheritdoc />
  public partial class OrganizationPatientcontractEmailupdated : Migration
  {
      /// <inheritdoc />
      protected override void Up(MigrationBuilder migrationBuilder)
      {
          migrationBuilder.RenameColumn(
              name: "defaultEmail",
              table: "Organization",
              newName: "contractEmail");
      }

      /// <inheritdoc />
      protected override void Down(MigrationBuilder migrationBuilder)
      {
          migrationBuilder.RenameColumn(
              name: "contractEmail",
              table: "Organization",
              newName: "defaultEmail");
      }
  }
