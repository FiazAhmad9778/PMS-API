using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.API.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddedSignatureColumn : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropColumn(
        name: "SignatureUrl",
        table: "User");

    migrationBuilder.AddColumn<byte[]>(
        name: "SignatureData",
        table: "User",
        type: "bytea",
        nullable: true);
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropColumn(
        name: "SignatureData",
        table: "User");

    migrationBuilder.AddColumn<string>(
        name: "SignatureUrl",
        table: "User",
        type: "text",
        nullable: true);
  }
}
