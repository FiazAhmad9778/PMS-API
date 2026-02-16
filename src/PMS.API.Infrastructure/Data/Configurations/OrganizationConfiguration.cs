using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PMS.API.Core.Domain.Entities;

namespace PMS.API.Infrastructure.Data.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
  public void Configure(EntityTypeBuilder<Organization> builder)
  {
    builder.ToTable("Organization");

    builder.HasKey(o => o.Id);
    builder.Property(o => o.Id)
        .HasColumnName("id")
        .ValueGeneratedOnAdd();

    builder.Property(o => o.OrganizationExternalId)
        .HasColumnName("organizationexternalid");

    builder.Property(o => o.Name)
        .HasColumnName("name")
        .IsRequired()
        .HasMaxLength(500);

    builder.Property(o => o.MinimumThreshold)
      .HasColumnName("minimumThreshold")
      .IsRequired();

    builder.Property(o => o.IsPatientRequired)
    .HasColumnName("isPatientRequired")
    .HasDefaultValue(false);

    builder.Property(o => o.CC)
     .HasColumnName("cc")
     .IsRequired()
     .HasMaxLength(1000);

    builder.Property(o => o.ContactName)
        .HasColumnName("contactName")
        .IsRequired()
        .HasMaxLength(500);

    builder.Property(o => o.Address)
        .HasColumnName("address")
        .IsRequired()
        .HasMaxLength(1000);

    builder.Property(o => o.ContractEmail)
        .HasColumnName("contractEmail")
        .HasMaxLength(255);

    builder.Property(o => o.CreatedDate)
        .HasColumnName("createdDate")
        .IsRequired();

    builder.Property(o => o.CreatedBy)
        .HasColumnName("createdBy");

    builder.Property(o => o.ModifiedDate)
        .HasColumnName("modifiedDate");

    builder.Property(o => o.ModifiedBy)
        .HasColumnName("modifiedBy");

    builder.Property(o => o.IsDeleted)
        .HasColumnName("isDeleted")
        .HasDefaultValue(false);

    // Configure one-to-many relationship with Ward
    builder.HasMany(o => o.Wards)
        .WithOne(w => w.Organization)
        .HasForeignKey(w => w.OrganizationId)
        .OnDelete(DeleteBehavior.SetNull);

    builder.HasIndex(o => o.Name);
  }
}
