using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PMS.API.Infrastructure.Migrations;

/// <inheritdoc />
public partial class InitialMigration : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.CreateTable(
        name: "ClaimGroup",
        columns: table => new
        {
          Id = table.Column<long>(type: "bigint", nullable: false)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
          Name = table.Column<string>(type: "text", nullable: true),
          IsDisplay = table.Column<bool>(type: "boolean", nullable: false),
          Sequence = table.Column<int>(type: "integer", nullable: false),
          CreatedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
          CreatedBy = table.Column<long>(type: "bigint", nullable: true),
          ModifiedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
          ModifiedBy = table.Column<long>(type: "bigint", nullable: true),
          IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_ClaimGroup", x => x.Id);
        });

    migrationBuilder.CreateTable(
        name: "PMSErrorLogs",
        columns: table => new
        {
          Id = table.Column<long>(type: "bigint", nullable: false)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
          Message = table.Column<string>(type: "text", nullable: true),
          ClientGroupId = table.Column<long>(type: "bigint", nullable: false),
          ClientId = table.Column<long>(type: "bigint", nullable: false),
          TenantId = table.Column<long>(type: "bigint", nullable: true),
          CreatedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
          CreatedBy = table.Column<long>(type: "bigint", nullable: true),
          ModifiedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
          ModifiedBy = table.Column<long>(type: "bigint", nullable: true),
          IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
          IsActive = table.Column<bool>(type: "boolean", nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_PMSErrorLogs", x => x.Id);
        });

    migrationBuilder.CreateTable(
        name: "Role",
        columns: table => new
        {
          Id = table.Column<long>(type: "bigint", nullable: false)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
          DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
          ClientGroupId = table.Column<long>(type: "bigint", nullable: false),
          IsSystem = table.Column<bool>(type: "boolean", nullable: false),
          Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
          NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
          ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Role", x => x.Id);
        });

    migrationBuilder.CreateTable(
        name: "User",
        columns: table => new
        {
          Id = table.Column<long>(type: "bigint", nullable: false)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
          FirstName = table.Column<string>(type: "text", nullable: false),
          LastName = table.Column<string>(type: "text", nullable: false),
          Address = table.Column<string>(type: "text", nullable: true),
          AvatarUrl = table.Column<string>(type: "text", nullable: true),
          SignatureUrl = table.Column<string>(type: "text", nullable: true),
          CreatedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
          CreatedBy = table.Column<long>(type: "bigint", nullable: true),
          ModifiedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
          ModifiedBy = table.Column<long>(type: "bigint", nullable: true),
          IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
          PhoneNumber = table.Column<string>(type: "text", nullable: true),
          IsActive = table.Column<bool>(type: "boolean", nullable: false),
          UserType = table.Column<int>(type: "integer", nullable: false),
          UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
          NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
          Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
          NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
          EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
          PasswordHash = table.Column<string>(type: "text", nullable: true),
          SecurityStamp = table.Column<string>(type: "text", nullable: true),
          ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
          PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
          TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
          LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
          LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
          AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_User", x => x.Id);
        });

    migrationBuilder.CreateTable(
        name: "ApplicationClaims",
        columns: table => new
        {
          Id = table.Column<long>(type: "bigint", nullable: false)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
          ClaimCode = table.Column<string>(type: "text", nullable: true),
          ClaimValue = table.Column<string>(type: "text", nullable: true),
          IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
          IsDisplay = table.Column<bool>(type: "boolean", nullable: false),
          ClaimGroupId = table.Column<long>(type: "bigint", nullable: false),
          IsAllowedToAll = table.Column<bool>(type: "boolean", nullable: false),
          AllowedSubscriptions = table.Column<List<int>>(type: "integer[]", nullable: true)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_ApplicationClaims", x => x.Id);
          table.ForeignKey(
                    name: "FK_ApplicationClaims_ClaimGroup_ClaimGroupId",
                    column: x => x.ClaimGroupId,
                    principalTable: "ClaimGroup",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
        });

    migrationBuilder.CreateTable(
        name: "AspNetUserLogins",
        columns: table => new
        {
          LoginProvider = table.Column<string>(type: "text", nullable: false),
          ProviderKey = table.Column<string>(type: "text", nullable: false),
          ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
          UserId = table.Column<long>(type: "bigint", nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
          table.ForeignKey(
                    name: "FK_AspNetUserLogins_User_UserId",
                    column: x => x.UserId,
                    principalTable: "User",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
        });

    migrationBuilder.CreateTable(
        name: "AspNetUserTokens",
        columns: table => new
        {
          UserId = table.Column<long>(type: "bigint", nullable: false),
          LoginProvider = table.Column<string>(type: "text", nullable: false),
          Name = table.Column<string>(type: "text", nullable: false),
          TokenExpiryTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
          Value = table.Column<string>(type: "text", nullable: true)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
          table.ForeignKey(
                    name: "FK_AspNetUserTokens_User_UserId",
                    column: x => x.UserId,
                    principalTable: "User",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
        });

    migrationBuilder.CreateTable(
        name: "UserClaim",
        columns: table => new
        {
          Id = table.Column<int>(type: "integer", nullable: false)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
          UserId = table.Column<long>(type: "bigint", nullable: false),
          ClaimType = table.Column<string>(type: "text", nullable: true),
          ClaimValue = table.Column<string>(type: "text", nullable: true)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_UserClaim", x => x.Id);
          table.ForeignKey(
                    name: "FK_UserClaim_User_UserId",
                    column: x => x.UserId,
                    principalTable: "User",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
        });

    migrationBuilder.CreateTable(
        name: "UserRole",
        columns: table => new
        {
          UserId = table.Column<long>(type: "bigint", nullable: false),
          RoleId = table.Column<long>(type: "bigint", nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_UserRole", x => new { x.UserId, x.RoleId });
          table.ForeignKey(
                    name: "FK_UserRole_Role_RoleId",
                    column: x => x.RoleId,
                    principalTable: "Role",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
          table.ForeignKey(
                    name: "FK_UserRole_User_UserId",
                    column: x => x.UserId,
                    principalTable: "User",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
        });

    migrationBuilder.CreateTable(
        name: "RoleClaim",
        columns: table => new
        {
          Id = table.Column<int>(type: "integer", nullable: false)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
          ApplicationClaimId = table.Column<long>(type: "bigint", nullable: false),
          IsAssigned = table.Column<bool>(type: "boolean", nullable: false),
          RoleId = table.Column<long>(type: "bigint", nullable: false),
          ClaimType = table.Column<string>(type: "text", nullable: true),
          ClaimValue = table.Column<string>(type: "text", nullable: true)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_RoleClaim", x => x.Id);
          table.ForeignKey(
                    name: "FK_RoleClaim_ApplicationClaims_ApplicationClaimId",
                    column: x => x.ApplicationClaimId,
                    principalTable: "ApplicationClaims",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
          table.ForeignKey(
                    name: "FK_RoleClaim_Role_RoleId",
                    column: x => x.RoleId,
                    principalTable: "Role",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
        });

    migrationBuilder.InsertData(
        table: "ClaimGroup",
        columns: new[] { "Id", "CreatedBy", "CreatedDate", "IsDeleted", "IsDisplay", "ModifiedBy", "ModifiedDate", "Name", "Sequence" },
        values: new object[,]
        {
                  { 1L, 1L, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, false, 1L, null, "User Management", 20 },
                  { 2L, 1L, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, false, 1L, null, "Reports Management", 40 }
        });

    migrationBuilder.InsertData(
        table: "Role",
        columns: new[] { "Id", "ClientGroupId", "ConcurrencyStamp", "DisplayName", "IsSystem", "Name", "NormalizedName" },
        values: new object[,]
        {
                  { 1L, 0L, null, "Super Admin", true, "Superadmin", "SUPERADMIN" },
                  { 2L, 0L, null, "Technician", false, "Technician", "TECHNICIAN" }
        });

    migrationBuilder.InsertData(
        table: "ApplicationClaims",
        columns: new[] { "Id", "AllowedSubscriptions", "ClaimCode", "ClaimGroupId", "ClaimValue", "IsAllowedToAll", "IsDeleted", "IsDisplay" },
        values: new object[,]
        {
                  { 1L, null, "CGMA", 1L, "User Management Add", false, false, false },
                  { 2L, null, "CGME", 1L, "User Management Edit", false, false, false },
                  { 3L, null, "CGMV", 1L, "User Management View", false, false, false },
                  { 4L, null, "CGMD", 1L, "User Management Delete", false, false, false },
                  { 5L, null, "RMV", 2L, "Report Management View", true, false, false },
                  { 6L, null, "RMS", 2L, "Report Management Signature", true, false, false },
                  { 7L, null, "RMD", 2L, "Report Management Download", true, false, false },
                  { 8L, null, "RMP", 2L, "Report Management Print", true, false, false }
        });

    migrationBuilder.InsertData(
        table: "RoleClaim",
        columns: new[] { "Id", "ApplicationClaimId", "ClaimType", "ClaimValue", "IsAssigned", "RoleId" },
        values: new object[,]
        {
                  { 1, 1L, null, null, true, 1L },
                  { 2, 2L, null, null, true, 1L },
                  { 3, 3L, null, null, true, 1L },
                  { 4, 4L, null, null, true, 1L },
                  { 5, 5L, null, null, true, 2L },
                  { 6, 6L, null, null, true, 2L },
                  { 7, 7L, null, null, true, 2L },
                  { 8, 8L, null, null, true, 2L }
        });

    migrationBuilder.CreateIndex(
        name: "IX_ApplicationClaims_ClaimGroupId",
        table: "ApplicationClaims",
        column: "ClaimGroupId");

    migrationBuilder.CreateIndex(
        name: "IX_AspNetUserLogins_UserId",
        table: "AspNetUserLogins",
        column: "UserId");

    migrationBuilder.CreateIndex(
        name: "RoleNameIndex",
        table: "Role",
        column: "NormalizedName");

    migrationBuilder.CreateIndex(
        name: "IX_RoleClaim_ApplicationClaimId",
        table: "RoleClaim",
        column: "ApplicationClaimId");

    migrationBuilder.CreateIndex(
        name: "IX_RoleClaim_RoleId",
        table: "RoleClaim",
        column: "RoleId");

    migrationBuilder.CreateIndex(
        name: "EmailIndex",
        table: "User",
        column: "NormalizedEmail");

    migrationBuilder.CreateIndex(
        name: "IX_User_Email",
        table: "User",
        column: "Email");

    migrationBuilder.CreateIndex(
        name: "IX_User_UserName",
        table: "User",
        column: "UserName");

    migrationBuilder.CreateIndex(
        name: "UserNameIndex",
        table: "User",
        column: "NormalizedUserName");

    migrationBuilder.CreateIndex(
        name: "IX_UserClaim_UserId",
        table: "UserClaim",
        column: "UserId");

    migrationBuilder.CreateIndex(
        name: "IX_UserRole_RoleId",
        table: "UserRole",
        column: "RoleId");
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropTable(
        name: "AspNetUserLogins");

    migrationBuilder.DropTable(
        name: "AspNetUserTokens");

    migrationBuilder.DropTable(
        name: "PMSErrorLogs");

    migrationBuilder.DropTable(
        name: "RoleClaim");

    migrationBuilder.DropTable(
        name: "UserClaim");

    migrationBuilder.DropTable(
        name: "UserRole");

    migrationBuilder.DropTable(
        name: "ApplicationClaims");

    migrationBuilder.DropTable(
        name: "Role");

    migrationBuilder.DropTable(
        name: "User");

    migrationBuilder.DropTable(
        name: "ClaimGroup");
  }
}
