using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using PMS.API.Core.Domain.Entities;

namespace PMS.API.Infrastructure.Data.Configurations;
public class PatientInvoiceHistoryConfiguration
    : IEntityTypeConfiguration<PatientInvoiceHistory>
{
  public void Configure(EntityTypeBuilder<PatientInvoiceHistory> builder)
  {
    builder.ToTable("PatientInvoiceHistory");

    builder.HasKey(p => p.Id);

    builder.Property(p => p.Id)
        .ValueGeneratedOnAdd();

    builder.Property(p => p.OrganizationId)
        .IsRequired();

    builder.Property(p => p.IsSent)
        .HasDefaultValue(false);

    builder.Property(p => p.InvoiceSendingWays)
        .HasMaxLength(200);

    builder.Property(p => p.InvoiceStartDate)
        .IsRequired();

    builder.Property(p => p.InvoiceEndDate)
        .IsRequired();

    builder.Property(p => p.FilePath)
        .HasMaxLength(1000);

    builder.Property(p => p.CreatedBy);

    builder.Property(p => p.CreatedDate)
        .IsRequired();

    builder.Property(p => p.ModifiedBy);

    builder.Property(p => p.ModifiedDate);

    builder.Property(p => p.IsDeleted)
        .HasDefaultValue(false);

    builder.HasIndex(p => new
    {
      p.OrganizationId,
      p.InvoiceStartDate,
      p.InvoiceEndDate
    });
  }
}

