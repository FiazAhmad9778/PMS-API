using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.API.Infrastructure.Migrations;

  /// <inheritdoc />
  public partial class PatientRelationupdated : Migration
  {
      /// <inheritdoc />
      protected override void Up(MigrationBuilder migrationBuilder)
      {
          migrationBuilder.AddColumn<long>(
              name: "WardId",
              table: "Patient",
              type: "bigint",
              nullable: true);

          migrationBuilder.CreateIndex(
              name: "IX_Patient_WardId",
              table: "Patient",
              column: "WardId");

          migrationBuilder.AddForeignKey(
              name: "FK_Patient_Ward_WardId",
              table: "Patient",
              column: "WardId",
              principalTable: "Ward",
              principalColumn: "id",
              onDelete: ReferentialAction.Cascade);
      }

      /// <inheritdoc />
      protected override void Down(MigrationBuilder migrationBuilder)
      {
          migrationBuilder.DropForeignKey(
              name: "FK_Patient_Ward_WardId",
              table: "Patient");

          migrationBuilder.DropIndex(
              name: "IX_Patient_WardId",
              table: "Patient");

          migrationBuilder.DropColumn(
              name: "WardId",
              table: "Patient");
      }
  }
