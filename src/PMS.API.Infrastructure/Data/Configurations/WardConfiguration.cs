using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PMS.API.Core.Domain.Entities;

namespace PMS.API.Infrastructure.Data.Configurations;

public class WardConfiguration : IEntityTypeConfiguration<Ward>
{
  public void Configure(EntityTypeBuilder<Ward> builder)
  {
    builder.ToTable("Ward");

    builder.HasKey(w => w.Id);
    builder.Property(w => w.Id)
        .HasColumnName("id")
        .ValueGeneratedOnAdd();

    builder.Property(w => w.Name)
        .HasColumnName("name")
        .IsRequired()
        .HasMaxLength(200);

    builder.Property(w => w.ExternalId)
        .HasColumnName("externalId")
        .HasMaxLength(100);

    builder.Property(w => w.OrganizationId)
        .HasColumnName("organizationId");

    builder.Property(w => w.CreatedDate)
        .HasColumnName("createdDate")
        .IsRequired();

    builder.Property(w => w.CreatedBy)
        .HasColumnName("createdBy");

    builder.Property(w => w.ModifiedDate)
        .HasColumnName("modifiedDate");

    builder.Property(w => w.ModifiedBy)
        .HasColumnName("modifiedBy");

    builder.Property(w => w.IsDeleted)
        .HasColumnName("isDeleted")
        .HasDefaultValue(false);

    builder.HasIndex(w => w.Name);
    builder.HasIndex(w => w.ExternalId);
    builder.HasIndex(w => w.OrganizationId);

    // Configure many-to-one relationship with Organization
    builder.HasOne(w => w.Organization)
        .WithMany(o => o.Wards)
        .HasForeignKey(w => w.OrganizationId)
        .OnDelete(DeleteBehavior.SetNull);
  }
}
