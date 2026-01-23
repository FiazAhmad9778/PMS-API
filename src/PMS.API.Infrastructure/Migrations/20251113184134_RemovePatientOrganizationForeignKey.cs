using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.API.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemovePatientOrganizationForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Patient_Organization_organizationId",
                table: "Patient");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_Patient_Organization_organizationId",
                table: "Patient",
                column: "organizationId",
                principalTable: "Organization",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
