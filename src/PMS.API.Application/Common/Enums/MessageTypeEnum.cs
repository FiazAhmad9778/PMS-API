using System.ComponentModel.DataAnnotations;

namespace PMS.API.Application.Common.Enums;
public enum MessageType { [Display(Name = "Email")] Email, [Display(Name = "SMS")] SMS, }
