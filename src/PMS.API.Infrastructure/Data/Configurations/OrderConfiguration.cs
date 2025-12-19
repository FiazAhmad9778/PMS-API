using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PMS.API.Core.Domain.Entities;

namespace PMS.API.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
  public void Configure(EntityTypeBuilder<Order> builder)
  {
    builder.ToTable("Order");

    builder.HasKey(o => o.Id);
    builder.Property(o => o.Id)
        .HasColumnName("id")
        .ValueGeneratedOnAdd();

    builder.Property(o => o.FirstName)
        .HasColumnName("firstName")
        .IsRequired()
        .HasMaxLength(200);

    builder.Property(o => o.LastName)
        .HasColumnName("lastName")
        .IsRequired()
        .HasMaxLength(200);

    builder.Property(o => o.PhoneNumber)
        .HasColumnName("phoneNumber")
        .IsRequired()
        .HasMaxLength(50);

    builder.Property(o => o.Medication)
        .HasColumnName("medication")
        .IsRequired()
        .HasMaxLength(1000);

    builder.Property(o => o.DeliveryOrPickup)
        .HasColumnName("deliveryOrPickup")
        .IsRequired()
        .HasMaxLength(50);

    builder.Property(o => o.Address)
        .HasColumnName("address")
        .HasMaxLength(1000);

    builder.Property(o => o.DeliveryTimeSlot)
        .HasColumnName("deliveryTimeSlot")
        .HasMaxLength(50);

    builder.Property(o => o.Notes)
        .HasColumnName("notes")
        .HasMaxLength(2000);

    builder.Property(o => o.FaxStatus)
        .HasColumnName("faxStatus")
        .HasMaxLength(50)
        .HasDefaultValue("Pending");

    builder.Property(o => o.FaxTransactionId)
        .HasColumnName("faxTransactionId")
        .HasMaxLength(200);

    builder.Property(o => o.FaxSentAt)
        .HasColumnName("faxSentAt");

    builder.Property(o => o.FaxRetryCount)
        .HasColumnName("faxRetryCount")
        .HasDefaultValue(0);

    builder.Property(o => o.FaxErrorMessage)
        .HasColumnName("faxErrorMessage")
        .HasMaxLength(1000);

    builder.Property(o => o.WebhookId)
        .HasColumnName("webhookId")
        .HasMaxLength(200);

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

    builder.HasIndex(o => o.CreatedDate);
    builder.HasIndex(o => o.FaxStatus);
    builder.HasIndex(o => new { o.FaxStatus, o.CreatedDate });
    builder.HasIndex(o => o.WebhookId).IsUnique();
  }
}


