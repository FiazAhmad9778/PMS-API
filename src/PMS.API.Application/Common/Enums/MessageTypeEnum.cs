using System.ComponentModel.DataAnnotations;

namespace PMS.API.Application.Common.Enums;
public enum MessageType { [Display(Name = "Email")] Email, [Display(Name = "SMS")] SMS, }

public enum InvoiceStatus
{
  [Display(Name = "Pending")]
  Pending = 1,
  [Display(Name = "In-Progress")]
  InProgress = 2,
  [Display(Name = "Invoiced")]
  Invoiced = 3
}
