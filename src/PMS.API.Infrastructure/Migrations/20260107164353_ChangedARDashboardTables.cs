using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PMS.API.Infrastructure.Migrations;

/// <inheritdoc />
public partial class ChangedARDashboardTables : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    // Drop foreign key constraint only if it exists
    migrationBuilder.Sql(@"
      DO $$ 
      BEGIN
        IF EXISTS (
          SELECT 1 
          FROM pg_constraint 
          WHERE conname = 'FK_Patient_Organization_organizationId'
        ) THEN
          ALTER TABLE ""Patient"" DROP CONSTRAINT ""FK_Patient_Organization_organizationId"";
        END IF;
      END $$;
    ");

    // Drop index only if it exists
    migrationBuilder.Sql(@"
      DO $$ 
      BEGIN
        IF EXISTS (
          SELECT 1 
          FROM pg_indexes 
          WHERE indexname = 'IX_Patient_organizationId'
        ) THEN
          DROP INDEX ""IX_Patient_organizationId"";
        END IF;
      END $$;
    ");

    // Drop columns only if they exist
    migrationBuilder.Sql(@"
      DO $$ 
      BEGIN
        IF EXISTS (
          SELECT 1 
          FROM information_schema.columns 
          WHERE table_name = 'Patient' AND column_name = 'organizationId'
        ) THEN
          ALTER TABLE ""Patient"" DROP COLUMN ""organizationId"";
        END IF;
      END $$;
    ");

    migrationBuilder.Sql(@"
      DO $$ 
      BEGIN
        IF EXISTS (
          SELECT 1 
          FROM information_schema.columns 
          WHERE table_name = 'Patient' AND column_name = 'organizationName'
        ) THEN
          ALTER TABLE ""Patient"" DROP COLUMN ""organizationName"";
        END IF;
      END $$;
    ");

    migrationBuilder.Sql(@"
      DO $$ 
      BEGIN
        IF EXISTS (
          SELECT 1 
          FROM information_schema.columns 
          WHERE table_name = 'Patient' AND column_name = 'ward'
        ) THEN
          ALTER TABLE ""Patient"" DROP COLUMN ""ward"";
        END IF;
      END $$;
    ");

    // Drop Ward column from Organization only if it exists
    migrationBuilder.Sql(@"
      DO $$ 
      BEGIN
        IF EXISTS (
          SELECT 1 
          FROM information_schema.columns 
          WHERE table_name = 'Organization' AND column_name = 'Ward'
        ) THEN
          ALTER TABLE ""Organization"" DROP COLUMN ""Ward"";
        END IF;
      END $$;
    ");

    migrationBuilder.RenameColumn(
        name: "Name",
        table: "Organization",
        newName: "name");

    migrationBuilder.RenameColumn(
        name: "ModifiedDate",
        table: "Organization",
        newName: "modifiedDate");

    migrationBuilder.RenameColumn(
        name: "ModifiedBy",
        table: "Organization",
        newName: "modifiedBy");

    migrationBuilder.RenameColumn(
        name: "IsDeleted",
        table: "Organization",
        newName: "isDeleted");

    migrationBuilder.RenameColumn(
        name: "DefaultEmail",
        table: "Organization",
        newName: "defaultEmail");

    migrationBuilder.RenameColumn(
        name: "CreatedDate",
        table: "Organization",
        newName: "createdDate");

    migrationBuilder.RenameColumn(
        name: "CreatedBy",
        table: "Organization",
        newName: "createdBy");

    migrationBuilder.RenameColumn(
        name: "Address",
        table: "Organization",
        newName: "address");

    migrationBuilder.RenameColumn(
        name: "Id",
        table: "Organization",
        newName: "id");

    migrationBuilder.RenameIndex(
        name: "IX_Organization_Name",
        table: "Organization",
        newName: "IX_Organization_name");

    migrationBuilder.AlterColumn<string>(
        name: "name",
        table: "Organization",
        type: "character varying(500)",
        maxLength: 500,
        nullable: false,
        oldClrType: typeof(string),
        oldType: "text");

    migrationBuilder.AlterColumn<bool>(
        name: "isDeleted",
        table: "Organization",
        type: "boolean",
        nullable: false,
        defaultValue: false,
        oldClrType: typeof(bool),
        oldType: "boolean");

    migrationBuilder.AlterColumn<string>(
        name: "defaultEmail",
        table: "Organization",
        type: "character varying(255)",
        maxLength: 255,
        nullable: true,
        oldClrType: typeof(string),
        oldType: "text",
        oldNullable: true);

    migrationBuilder.AlterColumn<string>(
        name: "address",
        table: "Organization",
        type: "character varying(1000)",
        maxLength: 1000,
        nullable: false,
        oldClrType: typeof(string),
        oldType: "text");

    migrationBuilder.CreateTable(
        name: "Ward",
        columns: table => new
        {
          id = table.Column<long>(type: "bigint", nullable: false)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
          name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
          externalId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
          organizationId = table.Column<long>(type: "bigint", nullable: true),
          createdDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
          createdBy = table.Column<long>(type: "bigint", nullable: true),
          modifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
          modifiedBy = table.Column<long>(type: "bigint", nullable: true),
          isDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Ward", x => x.id);
          table.ForeignKey(
                    name: "FK_Ward_Organization_organizationId",
                    column: x => x.organizationId,
                    principalTable: "Organization",
                    principalColumn: "id",
                    onDelete: ReferentialAction.SetNull);
        });

    migrationBuilder.CreateIndex(
        name: "IX_Ward_externalId",
        table: "Ward",
        column: "externalId");

    migrationBuilder.CreateIndex(
        name: "IX_Ward_name",
        table: "Ward",
        column: "name");

    migrationBuilder.CreateIndex(
        name: "IX_Ward_organizationId",
        table: "Ward",
        column: "organizationId");
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropTable(
        name: "Ward");

    migrationBuilder.RenameColumn(
        name: "name",
        table: "Organization",
        newName: "Name");

    migrationBuilder.RenameColumn(
        name: "modifiedDate",
        table: "Organization",
        newName: "ModifiedDate");

    migrationBuilder.RenameColumn(
        name: "modifiedBy",
        table: "Organization",
        newName: "ModifiedBy");

    migrationBuilder.RenameColumn(
        name: "isDeleted",
        table: "Organization",
        newName: "IsDeleted");

    migrationBuilder.RenameColumn(
        name: "defaultEmail",
        table: "Organization",
        newName: "DefaultEmail");

    migrationBuilder.RenameColumn(
        name: "createdDate",
        table: "Organization",
        newName: "CreatedDate");

    migrationBuilder.RenameColumn(
        name: "createdBy",
        table: "Organization",
        newName: "CreatedBy");

    migrationBuilder.RenameColumn(
        name: "address",
        table: "Organization",
        newName: "Address");

    migrationBuilder.RenameColumn(
        name: "id",
        table: "Organization",
        newName: "Id");

    migrationBuilder.RenameIndex(
        name: "IX_Organization_name",
        table: "Organization",
        newName: "IX_Organization_Name");

    migrationBuilder.AddColumn<long>(
        name: "organizationId",
        table: "Patient",
        type: "bigint",
        nullable: true);

    migrationBuilder.AddColumn<string>(
        name: "organizationName",
        table: "Patient",
        type: "character varying(500)",
        maxLength: 500,
        nullable: true);

    migrationBuilder.AddColumn<string>(
        name: "ward",
        table: "Patient",
        type: "character varying(200)",
        maxLength: 200,
        nullable: true);

    migrationBuilder.AlterColumn<string>(
        name: "Name",
        table: "Organization",
        type: "text",
        nullable: false,
        oldClrType: typeof(string),
        oldType: "character varying(500)",
        oldMaxLength: 500);

    migrationBuilder.AlterColumn<bool>(
        name: "IsDeleted",
        table: "Organization",
        type: "boolean",
        nullable: false,
        oldClrType: typeof(bool),
        oldType: "boolean",
        oldDefaultValue: false);

    migrationBuilder.AlterColumn<string>(
        name: "DefaultEmail",
        table: "Organization",
        type: "text",
        nullable: true,
        oldClrType: typeof(string),
        oldType: "character varying(255)",
        oldMaxLength: 255,
        oldNullable: true);

    migrationBuilder.AlterColumn<string>(
        name: "Address",
        table: "Organization",
        type: "text",
        nullable: false,
        oldClrType: typeof(string),
        oldType: "character varying(1000)",
        oldMaxLength: 1000);

    migrationBuilder.AddColumn<string>(
        name: "Ward",
        table: "Organization",
        type: "text",
        nullable: true);

    migrationBuilder.CreateIndex(
        name: "IX_Patient_organizationId",
        table: "Patient",
        column: "organizationId");

    migrationBuilder.AddForeignKey(
        name: "FK_Patient_Organization_organizationId",
        table: "Patient",
        column: "organizationId",
        principalTable: "Organization",
        principalColumn: "Id");
  }
}
