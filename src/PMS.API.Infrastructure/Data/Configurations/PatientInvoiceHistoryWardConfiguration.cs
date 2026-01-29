using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using PMS.API.Core.Domain.Entities;

namespace PMS.API.Infrastructure.Data.Configurations;
public class PatientInvoiceHistoryWardConfiguration
    : IEntityTypeConfiguration<PatientInvoiceHistoryWard>
{
  public void Configure(EntityTypeBuilder<PatientInvoiceHistoryWard> builder)
  {
    builder.ToTable("PatientInvoiceHistoryWard");

    builder.HasKey(w => w.Id);

    builder.Property(w => w.Id)
        .ValueGeneratedOnAdd();

    builder.Property(w => w.PatientInvoiceHistoryId)
        .IsRequired();

    builder.Property(w => w.WardId)
        .IsRequired();

    builder.Property(w => w.PatientIds)
        .HasMaxLength(2000);

    builder.HasOne(w => w.PatientInvoiceHistory)
        .WithMany(p => p.PatientInvoiceHistoryWardList)
        .HasForeignKey(w => w.PatientInvoiceHistoryId)
        .OnDelete(DeleteBehavior.Cascade);

    builder.HasIndex(w => w.WardId);
  }
}

