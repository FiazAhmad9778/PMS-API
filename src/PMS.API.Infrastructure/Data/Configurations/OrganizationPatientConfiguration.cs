using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using PMS.API.Core.Domain.Entities;

namespace PMS.API.Infrastructure.Data.Configurations;
public class OrganizationPatientConfiguration
    : IEntityTypeConfiguration<OrganizationPatient>
{
  public void Configure(EntityTypeBuilder<OrganizationPatient> builder)
  {
    builder.ToTable("OrganizationPatient");

    builder.HasKey(op => op.Id);

    builder.Property(op => op.Id)
        .HasColumnName("id")
        .ValueGeneratedOnAdd();

    builder.Property(op => op.Name)
        .HasColumnName("name")
        .IsRequired()
        .HasMaxLength(500);

    builder.Property(op => op.ExternalId)
        .HasColumnName("externalId")
        .IsRequired();

    builder.Property(op => op.OrganizationId)
        .HasColumnName("organizationId");

    builder.Property(op => op.WardId)
        .HasColumnName("wardId");

    builder.HasOne(op => op.Organization)
        .WithMany(o => o.OrganizationPatientList) 
        .HasForeignKey(op => op.OrganizationId)
        .OnDelete(DeleteBehavior.NoAction);

    builder.HasOne(op => op.Ward)
        .WithMany(w => w.OrganizationPatientList)
        .HasForeignKey(op => op.WardId)
        .OnDelete(DeleteBehavior.NoAction);

    builder.HasMany(op => op.InvoiceHistoryList)
        .WithOne(i => i.OrganizationPatient)
        .HasForeignKey(i => i.OrganizationPatientId)
        .OnDelete(DeleteBehavior.Cascade);

    builder.HasIndex(op => op.ExternalId);
    builder.HasIndex(op => op.Name);
  }
}
