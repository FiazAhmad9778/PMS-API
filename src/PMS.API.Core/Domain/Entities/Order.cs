using System.ComponentModel.DataAnnotations.Schema;
using PMS.API.Core.Domain.Interfaces;
using PMS.API.SharedKernel.Interfaces;

namespace PMS.API.Core.Domain.Entities;

[Table("Order")]
public class Order : IAggregateRoot, ISoftDeleteEntity, IAuditEntity
{
  public long Id { get; set; }
  
  // Personal Information
  public required string FirstName { get; set; }
  public required string LastName { get; set; }
  public required string PhoneNumber { get; set; }
  
  // Medication Information (JSON array of medication names)
  public required string Medication { get; set; } // JSON array: ["Medication 1", "Medication 2", ...]
  
  // Delivery/Pickup Information
  public required string DeliveryOrPickup { get; set; } // "Delivery" or "Pick up"
  public string? Address { get; set; }
  public string? DeliveryTimeSlot { get; set; } // "10AM", "1PM", "3PM"
  
  // Additional Information
  public string? Notes { get; set; }
  
  
  // Fax Processing Information
  public string? FaxStatus { get; set; } // "Pending", "Sent", "Failed", "Retrying"
  public string? FaxTransactionId { get; set; }
  public DateTime? FaxSentAt { get; set; }
  public int FaxRetryCount { get; set; } = 0;
  public string? FaxErrorMessage { get; set; }
  
  // Webhook Information (for idempotency)
  public string? WebhookId { get; set; } // Unique webhook ID from Webflow to prevent duplicate processing
  
  // Audit Fields
  public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
  public long? CreatedBy { get; set; }
  public DateTime? ModifiedDate { get; set; }
  public long? ModifiedBy { get; set; }
  public bool IsDeleted { get; set; }
}


