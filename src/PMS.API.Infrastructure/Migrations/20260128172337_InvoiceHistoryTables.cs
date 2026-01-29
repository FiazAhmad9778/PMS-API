using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PMS.API.Infrastructure.Migrations;

  /// <inheritdoc />
  public partial class InvoiceHistoryTables : Migration
  {
      /// <inheritdoc />
      protected override void Up(MigrationBuilder migrationBuilder)
      {
          migrationBuilder.CreateTable(
              name: "ClaimGroup",
              columns: table => new
              {
                  Id = table.Column<long>(type: "bigint", nullable: false)
                      .Annotation("SqlServer:Identity", "1, 1"),
                  Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                  IsDisplay = table.Column<bool>(type: "bit", nullable: false),
                  Sequence = table.Column<int>(type: "int", nullable: false),
                  CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                  CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                  ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                  ModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                  IsDeleted = table.Column<bool>(type: "bit", nullable: false)
              },
              constraints: table =>
              {
                  table.PrimaryKey("PK_ClaimGroup", x => x.Id);
              });

          migrationBuilder.CreateTable(
              name: "Document",
              columns: table => new
              {
                  Id = table.Column<long>(type: "bigint", nullable: false)
                      .Annotation("SqlServer:Identity", "1, 1"),
                  DocumentName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                  DocumentUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                  CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                  ModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                  CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                  ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                  Status = table.Column<int>(type: "int", nullable: false),
                  IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                  NoOfPatients = table.Column<int>(type: "int", nullable: false)
              },
              constraints: table =>
              {
                  table.PrimaryKey("PK_Document", x => x.Id);
              });

          migrationBuilder.CreateTable(
              name: "Order",
              columns: table => new
              {
                  id = table.Column<long>(type: "bigint", nullable: false)
                      .Annotation("SqlServer:Identity", "1, 1"),
                  firstName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                  lastName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                  phoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                  medication = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                  deliveryOrPickup = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                  address = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                  deliveryTimeSlot = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                  notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                  faxStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValue: "Pending"),
                  faxTransactionId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                  faxSentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                  faxRetryCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                  faxErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                  webhookId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                  createdDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                  createdBy = table.Column<long>(type: "bigint", nullable: true),
                  modifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                  modifiedBy = table.Column<long>(type: "bigint", nullable: true),
                  isDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
              },
              constraints: table =>
              {
                  table.PrimaryKey("PK_Order", x => x.id);
              });

          migrationBuilder.CreateTable(
              name: "Organization",
              columns: table => new
              {
                  id = table.Column<long>(type: "bigint", nullable: false)
                      .Annotation("SqlServer:Identity", "1, 1"),
                  name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                  address = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                  defaultEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                  createdDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                  createdBy = table.Column<long>(type: "bigint", nullable: true),
                  modifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                  modifiedBy = table.Column<long>(type: "bigint", nullable: true),
                  isDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
              },
              constraints: table =>
              {
                  table.PrimaryKey("PK_Organization", x => x.id);
              });

          migrationBuilder.CreateTable(
              name: "Patient",
              columns: table => new
              {
                  id = table.Column<long>(type: "bigint", nullable: false)
                      .Annotation("SqlServer:Identity", "1, 1"),
                  patientId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                  name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                  patientCreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                  defaultEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                  address = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                  status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "active"),
                  createdBy = table.Column<long>(type: "bigint", nullable: true),
                  modifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                  modifiedBy = table.Column<long>(type: "bigint", nullable: true),
                  isDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
              },
              constraints: table =>
              {
                  table.PrimaryKey("PK_Patient", x => x.id);
              });

          migrationBuilder.CreateTable(
              name: "PatientInvoiceHistory",
              columns: table => new
              {
                  PatientInvoiceHistoryId = table.Column<long>(type: "bigint", nullable: false)
                      .Annotation("SqlServer:Identity", "1, 1"),
                  OrganizationId = table.Column<long>(type: "bigint", nullable: false),
                  InvoiceSendingWays = table.Column<string>(type: "nvarchar(max)", nullable: true),
                  InvoiceStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                  InvoiceEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                  FilePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                  CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                  CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                  ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                  ModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                  IsDeleted = table.Column<bool>(type: "bit", nullable: false)
              },
              constraints: table =>
              {
                  table.PrimaryKey("PK_PatientInvoiceHistory", x => x.PatientInvoiceHistoryId);
              });

          migrationBuilder.CreateTable(
              name: "PMSErrorLogs",
              columns: table => new
              {
                  Id = table.Column<long>(type: "bigint", nullable: false)
                      .Annotation("SqlServer:Identity", "1, 1"),
                  Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                  ClientGroupId = table.Column<long>(type: "bigint", nullable: false),
                  ClientId = table.Column<long>(type: "bigint", nullable: false),
                  TenantId = table.Column<long>(type: "bigint", nullable: true),
                  CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                  CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                  ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                  ModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                  IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                  IsActive = table.Column<bool>(type: "bit", nullable: false)
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
                      .Annotation("SqlServer:Identity", "1, 1"),
                  DisplayName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                  ClientGroupId = table.Column<long>(type: "bigint", nullable: false),
                  IsSystem = table.Column<bool>(type: "bit", nullable: false),
                  Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                  NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                  ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                      .Annotation("SqlServer:Identity", "1, 1"),
                  FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                  LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                  Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                  AvatarUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                  SignatureData = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                  CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                  CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                  ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                  ModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                  IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                  PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                  IsActive = table.Column<bool>(type: "bit", nullable: false),
                  UserType = table.Column<int>(type: "int", nullable: false),
                  UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                  NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                  Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                  NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                  EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                  PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                  SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                  ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                  PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                  TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                  LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                  LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                  AccessFailedCount = table.Column<int>(type: "int", nullable: false)
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
                      .Annotation("SqlServer:Identity", "1, 1"),
                  ClaimCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                  ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                  IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                  IsDisplay = table.Column<bool>(type: "bit", nullable: false),
                  ClaimGroupId = table.Column<long>(type: "bigint", nullable: false),
                  IsAllowedToAll = table.Column<bool>(type: "bit", nullable: false),
                  AllowedSubscriptions = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
              name: "DocumentMetadata",
              columns: table => new
              {
                  Id = table.Column<long>(type: "bigint", nullable: false)
                      .Annotation("SqlServer:Identity", "1, 1"),
                  DocumentId = table.Column<long>(type: "bigint", nullable: false),
                  Key = table.Column<string>(type: "nvarchar(450)", nullable: false),
                  Value = table.Column<string>(type: "nvarchar(450)", nullable: false),
                  CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
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

          migrationBuilder.CreateTable(
              name: "Ward",
              columns: table => new
              {
                  id = table.Column<long>(type: "bigint", nullable: false)
                      .Annotation("SqlServer:Identity", "1, 1"),
                  name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                  externalId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                  organizationId = table.Column<long>(type: "bigint", nullable: true),
                  createdDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                  createdBy = table.Column<long>(type: "bigint", nullable: true),
                  modifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                  modifiedBy = table.Column<long>(type: "bigint", nullable: true),
                  isDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
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

          migrationBuilder.CreateTable(
              name: "PatientInvoiceHistoryWard",
              columns: table => new
              {
                  PatientInvoiceHistoryWardId = table.Column<long>(type: "bigint", nullable: false)
                      .Annotation("SqlServer:Identity", "1, 1"),
                  PatientInvoiceHistoryId = table.Column<long>(type: "bigint", nullable: false),
                  WardId = table.Column<long>(type: "bigint", nullable: false),
                  PatientIds = table.Column<string>(type: "nvarchar(max)", nullable: true)
              },
              constraints: table =>
              {
                  table.PrimaryKey("PK_PatientInvoiceHistoryWard", x => x.PatientInvoiceHistoryWardId);
                  table.ForeignKey(
                      name: "FK_PatientInvoiceHistoryWard_PatientInvoiceHistory_PatientInvoiceHistoryId",
                      column: x => x.PatientInvoiceHistoryId,
                      principalTable: "PatientInvoiceHistory",
                      principalColumn: "PatientInvoiceHistoryId",
                      onDelete: ReferentialAction.Cascade);
              });

          migrationBuilder.CreateTable(
              name: "AspNetUserLogins",
              columns: table => new
              {
                  LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                  ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                  ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                  LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                  Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                  TokenExpiryTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                  Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                  Id = table.Column<int>(type: "int", nullable: false)
                      .Annotation("SqlServer:Identity", "1, 1"),
                  UserId = table.Column<long>(type: "bigint", nullable: false),
                  ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                  ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                  Id = table.Column<int>(type: "int", nullable: false)
                      .Annotation("SqlServer:Identity", "1, 1"),
                  ApplicationClaimId = table.Column<long>(type: "bigint", nullable: false),
                  IsAssigned = table.Column<bool>(type: "bit", nullable: false),
                  RoleId = table.Column<long>(type: "bigint", nullable: false),
                  ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                  ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                  { 1L, 1L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, false, 1L, null, "User Management", 20 },
                  { 2L, 1L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, false, 1L, null, "Reports Management", 40 }
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

          migrationBuilder.CreateIndex(
              name: "IX_Order_createdDate",
              table: "Order",
              column: "createdDate");

          migrationBuilder.CreateIndex(
              name: "IX_Order_faxStatus",
              table: "Order",
              column: "faxStatus");

          migrationBuilder.CreateIndex(
              name: "IX_Order_faxStatus_createdDate",
              table: "Order",
              columns: new[] { "faxStatus", "createdDate" });

          migrationBuilder.CreateIndex(
              name: "IX_Order_webhookId",
              table: "Order",
              column: "webhookId",
              unique: true,
              filter: "[webhookId] IS NOT NULL");

          migrationBuilder.CreateIndex(
              name: "IX_Organization_name",
              table: "Organization",
              column: "name");

          migrationBuilder.CreateIndex(
              name: "IX_Patient_name",
              table: "Patient",
              column: "name");

          migrationBuilder.CreateIndex(
              name: "IX_Patient_patientId",
              table: "Patient",
              column: "patientId");

          migrationBuilder.CreateIndex(
              name: "IX_Patient_status",
              table: "Patient",
              column: "status");

          migrationBuilder.CreateIndex(
              name: "IX_PatientInvoiceHistoryWard_PatientInvoiceHistoryId",
              table: "PatientInvoiceHistoryWard",
              column: "PatientInvoiceHistoryId");

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
              name: "AspNetUserLogins");

          migrationBuilder.DropTable(
              name: "AspNetUserTokens");

          migrationBuilder.DropTable(
              name: "DocumentMetadata");

          migrationBuilder.DropTable(
              name: "Order");

          migrationBuilder.DropTable(
              name: "Patient");

          migrationBuilder.DropTable(
              name: "PatientInvoiceHistoryWard");

          migrationBuilder.DropTable(
              name: "PMSErrorLogs");

          migrationBuilder.DropTable(
              name: "RoleClaim");

          migrationBuilder.DropTable(
              name: "UserClaim");

          migrationBuilder.DropTable(
              name: "UserRole");

          migrationBuilder.DropTable(
              name: "Ward");

          migrationBuilder.DropTable(
              name: "Document");

          migrationBuilder.DropTable(
              name: "PatientInvoiceHistory");

          migrationBuilder.DropTable(
              name: "ApplicationClaims");

          migrationBuilder.DropTable(
              name: "Role");

          migrationBuilder.DropTable(
              name: "User");

          migrationBuilder.DropTable(
              name: "Organization");

          migrationBuilder.DropTable(
              name: "ClaimGroup");
      }
  }
