using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PMS.API.Core.Domain.Entities;

namespace PMS.API.Infrastructure.Data.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
  public void Configure(EntityTypeBuilder<Patient> builder)
  {
    builder.ToTable("Patient");

    builder.HasKey(p => p.Id);
    builder.Property(p => p.Id)
        .HasColumnName("id")
        .ValueGeneratedOnAdd();

    builder.Property(p => p.PatientId)
        .HasColumnName("patientId");

    builder.Property(p => p.Name)
        .HasColumnName("name")
        .IsRequired()
        .HasMaxLength(500);

    builder.Property(p => p.CreatedDate)
        .HasColumnName("patientCreatedDate")
        .IsRequired();

    builder.Property(p => p.DefaultEmail)
        .HasColumnName("defaultEmail")
        .HasMaxLength(255);

    builder.Property(p => p.Address)
        .HasColumnName("address")
        .HasMaxLength(1000);

    builder.Property(p => p.Status)
        .HasColumnName("status")
        .HasMaxLength(50)
        .HasDefaultValue("active");

    builder.Property(p => p.CreatedBy)
        .HasColumnName("createdBy");

    builder.Property(p => p.ModifiedDate)
        .HasColumnName("modifiedDate");

    builder.Property(p => p.ModifiedBy)
        .HasColumnName("modifiedBy");

    builder.Property(p => p.IsDeleted)
        .HasColumnName("isDeleted")
        .HasDefaultValue(false);

    builder.HasIndex(p => p.Name);
    builder.HasIndex(p => p.PatientId);
    builder.HasIndex(p => p.Status);

   
  }
}

