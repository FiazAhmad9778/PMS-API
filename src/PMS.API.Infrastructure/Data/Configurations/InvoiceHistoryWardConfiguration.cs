using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PMS.API.Core.Domain.Entities;

namespace PMS.API.Infrastructure.Data.Configurations;

public class InvoiceHistoryWardConfiguration : IEntityTypeConfiguration<InvoiceHistoryWard>
{
  public void Configure(EntityTypeBuilder<InvoiceHistoryWard> builder)
  {
    builder.ToTable("InvoiceHistoryWard");

    builder.HasKey(w => w.Id);

    builder.Property(w => w.Id)
        .ValueGeneratedOnAdd();

    builder.Property(w => w.InvoiceHistoryId)
        .IsRequired();

    builder.Property(w => w.WardId)
        .IsRequired();

    builder.Property(w => w.PatientIds)
        .HasMaxLength(2000);

    builder.HasOne(w => w.InvoiceHistory)
        .WithMany(p => p.InvoiceHistoryWardList)
        .HasForeignKey(w => w.InvoiceHistoryId)
        .OnDelete(DeleteBehavior.Cascade);

    builder.HasIndex(w => w.InvoiceHistoryId);
    builder.HasIndex(w => w.WardId);
  }
}
