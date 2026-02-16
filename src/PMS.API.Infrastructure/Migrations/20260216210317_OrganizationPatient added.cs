using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PMS.API.Infrastructure.Migrations;

  /// <inheritdoc />
  public partial class OrganizationPatientadded : Migration
  {
      /// <inheritdoc />
      protected override void Up(MigrationBuilder migrationBuilder)
      {
          migrationBuilder.AddColumn<string>(
              name: "cc",
              table: "Organization",
              type: "character varying(1000)",
              maxLength: 1000,
              nullable: false,
              defaultValue: "");

          migrationBuilder.AddColumn<string>(
              name: "contactName",
              table: "Organization",
              type: "character varying(500)",
              maxLength: 500,
              nullable: false,
              defaultValue: "");

          migrationBuilder.AddColumn<bool>(
              name: "isPatientRequired",
              table: "Organization",
              type: "boolean",
              nullable: false,
              defaultValue: false);

          migrationBuilder.AddColumn<int>(
              name: "minimumThreshold",
              table: "Organization",
              type: "integer",
              nullable: false,
              defaultValue: 0);

          migrationBuilder.AddColumn<long>(
              name: "OrganizationPatientId",
              table: "InvoiceHistory",
              type: "bigint",
              nullable: true);

          migrationBuilder.CreateTable(
              name: "OrganizationPatient",
              columns: table => new
              {
                  id = table.Column<long>(type: "bigint", nullable: false)
                      .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                  name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                  externalId = table.Column<long>(type: "bigint", nullable: false),
                  organizationId = table.Column<long>(type: "bigint", nullable: true),
                  wardId = table.Column<long>(type: "bigint", nullable: true)
              },
              constraints: table =>
              {
                  table.PrimaryKey("PK_OrganizationPatient", x => x.id);
                  table.ForeignKey(
                      name: "FK_OrganizationPatient_Organization_organizationId",
                      column: x => x.organizationId,
                      principalTable: "Organization",
                      principalColumn: "id");
                  table.ForeignKey(
                      name: "FK_OrganizationPatient_Ward_wardId",
                      column: x => x.wardId,
                      principalTable: "Ward",
                      principalColumn: "id");
              });

          migrationBuilder.CreateIndex(
              name: "IX_InvoiceHistory_OrganizationPatientId",
              table: "InvoiceHistory",
              column: "OrganizationPatientId");

          migrationBuilder.CreateIndex(
              name: "IX_OrganizationPatient_externalId",
              table: "OrganizationPatient",
              column: "externalId");

          migrationBuilder.CreateIndex(
              name: "IX_OrganizationPatient_name",
              table: "OrganizationPatient",
              column: "name");

          migrationBuilder.CreateIndex(
              name: "IX_OrganizationPatient_organizationId",
              table: "OrganizationPatient",
              column: "organizationId");

          migrationBuilder.CreateIndex(
              name: "IX_OrganizationPatient_wardId",
              table: "OrganizationPatient",
              column: "wardId");

          migrationBuilder.AddForeignKey(
              name: "FK_InvoiceHistory_OrganizationPatient_OrganizationPatientId",
              table: "InvoiceHistory",
              column: "OrganizationPatientId",
              principalTable: "OrganizationPatient",
              principalColumn: "id",
              onDelete: ReferentialAction.Cascade);
      }

      /// <inheritdoc />
      protected override void Down(MigrationBuilder migrationBuilder)
      {
          migrationBuilder.DropForeignKey(
              name: "FK_InvoiceHistory_OrganizationPatient_OrganizationPatientId",
              table: "InvoiceHistory");

          migrationBuilder.DropTable(
              name: "OrganizationPatient");

          migrationBuilder.DropIndex(
              name: "IX_InvoiceHistory_OrganizationPatientId",
              table: "InvoiceHistory");

          migrationBuilder.DropColumn(
              name: "cc",
              table: "Organization");

          migrationBuilder.DropColumn(
              name: "contactName",
              table: "Organization");

          migrationBuilder.DropColumn(
              name: "isPatientRequired",
              table: "Organization");

          migrationBuilder.DropColumn(
              name: "minimumThreshold",
              table: "Organization");

          migrationBuilder.DropColumn(
              name: "OrganizationPatientId",
              table: "InvoiceHistory");
      }
  }
