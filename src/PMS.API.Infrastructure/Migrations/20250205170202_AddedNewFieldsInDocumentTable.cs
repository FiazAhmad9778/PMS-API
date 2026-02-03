using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PMS.API.Infrastructure.Migrations;

public partial class AddedNewFieldsInDocumentTable : Migration
{
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.AlterColumn<DateTime>(
        name: "ModifiedDate",
        table: "User",
        type: "timestamp without time zone",
        nullable: true,
        oldClrType: typeof(DateTimeOffset),
        oldType: "timestamp with time zone",
        oldNullable: true);

    migrationBuilder.AlterColumn<DateTime>(
        name: "CreatedDate",
        table: "User",
        type: "timestamp without time zone",
        nullable: false,
        oldClrType: typeof(DateTimeOffset),
        oldType: "timestamp with time zone");

    migrationBuilder.AlterColumn<DateTime>(
        name: "ModifiedDate",
        table: "PMSErrorLogs",
        type: "timestamp without time zone",
        nullable: true,
        oldClrType: typeof(DateTimeOffset),
        oldType: "timestamp with time zone",
        oldNullable: true);

    migrationBuilder.AlterColumn<DateTime>(
        name: "CreatedDate",
        table: "PMSErrorLogs",
        type: "timestamp without time zone",
        nullable: false,
        oldClrType: typeof(DateTimeOffset),
        oldType: "timestamp with time zone");

    migrationBuilder.AlterColumn<DateTime>(
        name: "ModifiedDate",
        table: "ClaimGroup",
        type: "timestamp without time zone",
        nullable: true,
        oldClrType: typeof(DateTimeOffset),
        oldType: "timestamp with time zone",
        oldNullable: true);

    migrationBuilder.AlterColumn<DateTime>(
        name: "CreatedDate",
        table: "ClaimGroup",
        type: "timestamp without time zone",
        nullable: false,
        oldClrType: typeof(DateTimeOffset),
        oldType: "timestamp with time zone");

    migrationBuilder.AlterColumn<DateTime>(
        name: "TokenExpiryTime",
        table: "AspNetUserTokens",
        type: "timestamp without time zone",
        nullable: true,
        oldClrType: typeof(DateTimeOffset),
        oldType: "timestamp with time zone",
        oldNullable: true);

    migrationBuilder.CreateTable(
        name: "Document",
        columns: table => new
        {
          Id = table.Column<long>(type: "bigint", nullable: false)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
          DocumentName = table.Column<string>(type: "text", nullable: false),
          DocumentUrl = table.Column<string>(type: "text", nullable: false),
          CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
          ModifiedBy = table.Column<long>(type: "bigint", nullable: true),
          CreatedBy = table.Column<long>(type: "bigint", nullable: true),
          ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
          Status = table.Column<int>(type: "integer", nullable: false),
          IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
          NoOfPatients = table.Column<int>(type: "integer", nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Document", x => x.Id);
        });

    migrationBuilder.CreateTable(
        name: "DocumentMetadata",
        columns: table => new
        {
          Id = table.Column<long>(type: "bigint", nullable: false)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
          DocumentId = table.Column<long>(type: "bigint", nullable: false),
          Key = table.Column<string>(type: "text", nullable: false),
          Value = table.Column<string>(type: "text", nullable: false),
          CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_DocumentMetadata", x => x.Id);
          table.ForeignKey(
                    name: "FK_DocumentMetadata_Document_DocumentId",
                    column: x => x.DocumentId,
                    principalTable: "Document",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
        });

    migrationBuilder.UpdateData(
        table: "ClaimGroup",
        keyColumn: "Id",
        keyValue: 1L,
        columns: new[] { "CreatedDate", "ModifiedDate" },
        values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null });

    migrationBuilder.UpdateData(
        table: "ClaimGroup",
        keyColumn: "Id",
        keyValue: 2L,
        columns: new[] { "CreatedDate", "ModifiedDate" },
        values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null });

    migrationBuilder.CreateIndex(
        name: "IX_Document_Status",
        table: "Document",
        column: "Status");

    migrationBuilder.CreateIndex(
        name: "IX_Document_Status_CreatedDate",
        table: "Document",
        columns: new[] { "Status", "CreatedDate" });

    migrationBuilder.CreateIndex(
        name: "IX_DocumentMetadata_CreatedDate",
        table: "DocumentMetadata",
        column: "CreatedDate");

    migrationBuilder.CreateIndex(
        name: "IX_DocumentMetadata_DocumentId",
        table: "DocumentMetadata",
        column: "DocumentId");

    migrationBuilder.CreateIndex(
        name: "IX_DocumentMetadata_Key_Value",
        table: "DocumentMetadata",
        columns: new[] { "Key", "Value" });
  }

  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropTable(
        name: "DocumentMetadata");

    migrationBuilder.DropTable(
        name: "Document");

    migrationBuilder.AlterColumn<DateTimeOffset>(
        name: "ModifiedDate",
        table: "User",
        type: "timestamp with time zone",
        nullable: true,
        oldClrType: typeof(DateTime),
        oldType: "timestamp without time zone",
        oldNullable: true);

    migrationBuilder.AlterColumn<DateTimeOffset>(
        name: "CreatedDate",
        table: "User",
        type: "timestamp with time zone",
        nullable: false,
        oldClrType: typeof(DateTime),
        oldType: "timestamp without time zone");

    migrationBuilder.AlterColumn<DateTimeOffset>(
        name: "ModifiedDate",
        table: "PMSErrorLogs",
        type: "timestamp with time zone",
        nullable: true,
        oldClrType: typeof(DateTime),
        oldType: "timestamp without time zone",
        oldNullable: true);

    migrationBuilder.AlterColumn<DateTimeOffset>(
        name: "CreatedDate",
        table: "PMSErrorLogs",
        type: "timestamp with time zone",
        nullable: false,
        oldClrType: typeof(DateTime),
        oldType: "timestamp without time zone");

    migrationBuilder.AlterColumn<DateTimeOffset>(
        name: "ModifiedDate",
        table: "ClaimGroup",
        type: "timestamp with time zone",
        nullable: true,
        oldClrType: typeof(DateTime),
        oldType: "timestamp without time zone",
        oldNullable: true);

    migrationBuilder.AlterColumn<DateTimeOffset>(
        name: "CreatedDate",
        table: "ClaimGroup",
        type: "timestamp with time zone",
        nullable: false,
        oldClrType: typeof(DateTime),
        oldType: "timestamp without time zone");

    migrationBuilder.AlterColumn<DateTimeOffset>(
        name: "TokenExpiryTime",
        table: "AspNetUserTokens",
        type: "timestamp with time zone",
        nullable: true,
        oldClrType: typeof(DateTime),
        oldType: "timestamp without time zone",
        oldNullable: true);

    migrationBuilder.UpdateData(
        table: "ClaimGroup",
        keyColumn: "Id",
        keyValue: 1L,
        columns: new[] { "CreatedDate", "ModifiedDate" },
        values: new object[] { new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null });

    migrationBuilder.UpdateData(
        table: "ClaimGroup",
        keyColumn: "Id",
        keyValue: 2L,
        columns: new[] { "CreatedDate", "ModifiedDate" },
        values: new object[] { new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null });
  }
}
