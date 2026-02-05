using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PMS.API.Core.Domain.Entities;

namespace PMS.API.Infrastructure.Data.Configurations;

public class InvoiceHistoryConfiguration : IEntityTypeConfiguration<InvoiceHistory>
{
  public void Configure(EntityTypeBuilder<InvoiceHistory> builder)
  {
    builder.ToTable("InvoiceHistory");

    builder.HasKey(p => p.Id);

    builder.Property(p => p.Id)
        .ValueGeneratedOnAdd();

    builder.Property(p => p.OrganizationId)
        .IsRequired(false);

    builder.Property(p => p.PatientId)
        .IsRequired(false);

    builder.Property(p => p.IsSent);

    builder.Property(p => p.InvoiceStatus)
        .HasMaxLength(50);

    builder.Property(p => p.InvoiceStatusHistory)
        .HasMaxLength(4000);

    builder.Property(p => p.InvoiceStartDate)
        .IsRequired();

    builder.Property(p => p.InvoiceEndDate)
        .IsRequired();

    builder.Property(p => p.FilePath)
        .HasMaxLength(1000);

    builder.Property(p => p.CreatedBy);
    builder.Property(p => p.CreatedDate).IsRequired();
    builder.Property(p => p.ModifiedBy);
    builder.Property(p => p.ModifiedDate);
    builder.Property(p => p.IsDeleted);

    builder.HasIndex(p => new { p.OrganizationId, p.InvoiceStartDate, p.InvoiceEndDate });
    builder.HasIndex(p => p.InvoiceStatus);

    builder.HasOne(x => x.Patient)
     .WithMany(x => x.InvoiceHistoryList)
     .HasForeignKey(x => x.PatientId)
     .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(x => x.Organization)
    .WithMany(x => x.InvoiceHistoryList)
    .HasForeignKey(x => x.OrganizationId)
    .OnDelete(DeleteBehavior.Cascade);
  }
}
