﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PMS.API.Infrastructure.Data;

namespace PMS.API.Infrastructure.Migrations
{
  [DbContext(typeof(AppDbContext))]
    [Migration("20250205170202_AddedNewFieldsInDocumentTable")]
    partial class AddedNewFieldsInDocumentTable
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("PMS.API.Core.Domain.Entities.Document", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long?>("CreatedBy")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("DocumentName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("DocumentUrl")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<long?>("ModifiedBy")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("NoOfPatients")
                        .HasColumnType("integer");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("Status");

                    b.HasIndex("Status", "CreatedDate");

                    b.ToTable("Document");
                });

            modelBuilder.Entity("PMS.API.Core.Domain.Entities.DocumentMetadata", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<long>("DocumentId")
                        .HasColumnType("bigint");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("CreatedDate");

                    b.HasIndex("DocumentId");

                    b.HasIndex("Key", "Value");

                    b.ToTable("DocumentMetadata");
                });

            modelBuilder.Entity("PMS.API.Core.Domain.Entities.Identity.ApplicationClaim", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.PrimitiveCollection<List<int>>("AllowedSubscriptions")
                        .HasColumnType("integer[]");

                    b.Property<string>("ClaimCode")
                        .HasColumnType("text");

                    b.Property<long>("ClaimGroupId")
                        .HasColumnType("bigint");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<bool>("IsAllowedToAll")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsDisplay")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.HasIndex("ClaimGroupId");

                    b.ToTable("ApplicationClaims");

                    b.HasData(
                        new
                        {
                            Id = 1L,
                            ClaimCode = "CGMA",
                            ClaimGroupId = 1L,
                            ClaimValue = "User Management Add",
                            IsAllowedToAll = false,
                            IsDeleted = false,
                            IsDisplay = false
                        },
                        new
                        {
                            Id = 2L,
                            ClaimCode = "CGME",
                            ClaimGroupId = 1L,
                            ClaimValue = "User Management Edit",
                            IsAllowedToAll = false,
                            IsDeleted = false,
                            IsDisplay = false
                        },
                        new
                        {
                            Id = 3L,
                            ClaimCode = "CGMV",
                            ClaimGroupId = 1L,
                            ClaimValue = "User Management View",
                            IsAllowedToAll = false,
                            IsDeleted = false,
                            IsDisplay = false
                        },
                        new
                        {
                            Id = 4L,
                            ClaimCode = "CGMD",
                            ClaimGroupId = 1L,
                            ClaimValue = "User Management Delete",
                            IsAllowedToAll = false,
                            IsDeleted = false,
                            IsDisplay = false
                        },
                        new
                        {
                            Id = 5L,
                            ClaimCode = "RMV",
                            ClaimGroupId = 2L,
                            ClaimValue = "Report Management View",
                            IsAllowedToAll = true,
                            IsDeleted = false,
                            IsDisplay = false
                        },
                        new
                        {
                            Id = 6L,
                            ClaimCode = "RMS",
                            ClaimGroupId = 2L,
                            ClaimValue = "Report Management Signature",
                            IsAllowedToAll = true,
                            IsDeleted = false,
                            IsDisplay = false
                        },
                        new
                        {
                            Id = 7L,
                            ClaimCode = "RMD",
                            ClaimGroupId = 2L,
                            ClaimValue = "Report Management Download",
                            IsAllowedToAll = true,
                            IsDeleted = false,
                            IsDisplay = false
                        },
                        new
                        {
                            Id = 8L,
                            ClaimCode = "RMP",
                            ClaimGroupId = 2L,
                            ClaimValue = "Report Management Print",
                            IsAllowedToAll = true,
                            IsDeleted = false,
                            IsDisplay = false
                        });
                });

            modelBuilder.Entity("PMS.API.Core.Domain.Entities.Identity.ClaimGroup", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long?>("CreatedBy")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsDisplay")
                        .HasColumnType("boolean");

                    b.Property<long?>("ModifiedBy")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<int>("Sequence")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("ClaimGroup");

                    b.HasData(
                        new
                        {
                            Id = 1L,
                            CreatedBy = 1L,
                            CreatedDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            IsDeleted = false,
                            IsDisplay = false,
                            ModifiedBy = 1L,
                            Name = "User Management",
                            Sequence = 20
                        },
                        new
                        {
                            Id = 2L,
                            CreatedBy = 1L,
                            CreatedDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            IsDeleted = false,
                            IsDisplay = false,
                            ModifiedBy = 1L,
                            Name = "Reports Management",
                            Sequence = 40
                        });
                });

            modelBuilder.Entity("PMS.API.Core.Domain.Entities.Identity.Role", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long>("ClientGroupId")
                        .HasColumnType("bigint");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("DisplayName")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<bool>("IsSystem")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("Role", (string)null);

                    b.HasData(
                        new
                        {
                            Id = 1L,
                            ClientGroupId = 0L,
                            DisplayName = "Super Admin",
                            IsSystem = true,
                            Name = "Superadmin",
                            NormalizedName = "SUPERADMIN"
                        },
                        new
                        {
                            Id = 2L,
                            ClientGroupId = 0L,
                            DisplayName = "Technician",
                            IsSystem = false,
                            Name = "Technician",
                            NormalizedName = "TECHNICIAN"
                        });
                });

            modelBuilder.Entity("PMS.API.Core.Domain.Entities.Identity.RoleClaim", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<long>("ApplicationClaimId")
                        .HasColumnType("bigint");

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<bool>("IsAssigned")
                        .HasColumnType("boolean");

                    b.Property<long>("RoleId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("ApplicationClaimId");

                    b.HasIndex("RoleId");

                    b.ToTable("RoleClaim", (string)null);

                    b.HasData(
                        new
                        {
                            Id = 1,
                            ApplicationClaimId = 1L,
                            IsAssigned = true,
                            RoleId = 1L
                        },
                        new
                        {
                            Id = 2,
                            ApplicationClaimId = 2L,
                            IsAssigned = true,
                            RoleId = 1L
                        },
                        new
                        {
                            Id = 3,
                            ApplicationClaimId = 3L,
                            IsAssigned = true,
                            RoleId = 1L
                        },
                        new
                        {
                            Id = 4,
                            ApplicationClaimId = 4L,
                            IsAssigned = true,
                            RoleId = 1L
                        },
                        new
                        {
                            Id = 5,
                            ApplicationClaimId = 5L,
                            IsAssigned = true,
                            RoleId = 2L
                        },
                        new
                        {
                            Id = 6,
                            ApplicationClaimId = 6L,
                            IsAssigned = true,
                            RoleId = 2L
                        },
                        new
                        {
                            Id = 7,
                            ApplicationClaimId = 7L,
                            IsAssigned = true,
                            RoleId = 2L
                        },
                        new
                        {
                            Id = 8,
                            ApplicationClaimId = 8L,
                            IsAssigned = true,
                            RoleId = 2L
                        });
                });

            modelBuilder.Entity("PMS.API.Core.Domain.Entities.Identity.User", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("integer");

                    b.Property<string>("Address")
                        .HasColumnType("text");

                    b.Property<string>("AvatarUrl")
                        .HasColumnType("text");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<long?>("CreatedBy")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("boolean");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long?>("ModifiedBy")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("boolean");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("text");

                    b.Property<string>("SignatureUrl")
                        .HasColumnType("text");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("boolean");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<int>("UserType")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("Email");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .HasDatabaseName("UserNameIndex");

                    b.HasIndex("UserName");

                    b.ToTable("User", (string)null);
                });

            modelBuilder.Entity("PMS.API.Core.Domain.Entities.Identity.UserClaim", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("UserClaim", (string)null);
                });

            modelBuilder.Entity("PMS.API.Core.Domain.Entities.Identity.UserLogin", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("text");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("text");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("text");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("PMS.API.Core.Domain.Entities.Identity.UserRole", b =>
                {
                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.Property<long>("RoleId")
                        .HasColumnType("bigint");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("UserRole", (string)null);
                });

            modelBuilder.Entity("PMS.API.Core.Domain.Entities.Identity.UserToken", b =>
                {
                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<DateTime?>("TokenExpiryTime")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Value")
                        .HasColumnType("text");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("PMS.API.Core.Domain.Entities.PMSErrorLog", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long>("ClientGroupId")
                        .HasColumnType("bigint");

                    b.Property<long>("ClientId")
                        .HasColumnType("bigint");

                    b.Property<long?>("CreatedBy")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<string>("Message")
                        .HasColumnType("text");

                    b.Property<long?>("ModifiedBy")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<long?>("TenantId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("PMSErrorLogs");
                });

            modelBuilder.Entity("PMS.API.Core.Domain.Entities.DocumentMetadata", b =>
                {
                    b.HasOne("PMS.API.Core.Domain.Entities.Document", "Document")
                        .WithMany("Metadata")
                        .HasForeignKey("DocumentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Document");
                });

            modelBuilder.Entity("PMS.API.Core.Domain.Entities.Identity.ApplicationClaim", b =>
                {
                    b.HasOne("PMS.API.Core.Domain.Entities.Identity.ClaimGroup", "ClaimGroup")
                        .WithMany("ApplicationClaims")
                        .HasForeignKey("ClaimGroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ClaimGroup");
                });

            modelBuilder.Entity("PMS.API.Core.Domain.Entities.Identity.RoleClaim", b =>
                {
                    b.HasOne("PMS.API.Core.Domain.Entities.Identity.ApplicationClaim", "ApplicationClaim")
                        .WithMany()
                        .HasForeignKey("ApplicationClaimId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PMS.API.Core.Domain.Entities.Identity.Role", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ApplicationClaim");
                });

            modelBuilder.Entity("PMS.API.Core.Domain.Entities.Identity.UserClaim", b =>
                {
                    b.HasOne("PMS.API.Core.Domain.Entities.Identity.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("PMS.API.Core.Domain.Entities.Identity.UserLogin", b =>
                {
                    b.HasOne("PMS.API.Core.Domain.Entities.Identity.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("PMS.API.Core.Domain.Entities.Identity.UserRole", b =>
                {
                    b.HasOne("PMS.API.Core.Domain.Entities.Identity.Role", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PMS.API.Core.Domain.Entities.Identity.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("PMS.API.Core.Domain.Entities.Identity.UserToken", b =>
                {
                    b.HasOne("PMS.API.Core.Domain.Entities.Identity.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("PMS.API.Core.Domain.Entities.Document", b =>
                {
                    b.Navigation("Metadata");
                });

            modelBuilder.Entity("PMS.API.Core.Domain.Entities.Identity.ClaimGroup", b =>
                {
                    b.Navigation("ApplicationClaims");
                });
#pragma warning restore 612, 618
        }
    }
}
