using System.Text.Json;
using System.Text.Json.Serialization;

namespace PMS.API.Application.Features.Orders.DTOs;

/// <summary>
/// Root webhook payload from Webflow (array wrapper)
/// </summary>
public class WebflowWebhookRootDto
{
  [JsonPropertyName("triggerType")]
  public string? TriggerType { get; set; }

  [JsonPropertyName("payload")]
  public WebflowWebhookPayloadDto? Payload { get; set; }
}

/// <summary>
/// Payload wrapper containing the form data
/// </summary>
public class WebflowWebhookPayloadDto
{

  [JsonPropertyName("id")]
  public string? Id { get; set; } // Unique webhook ID for idempotency

  [JsonPropertyName("publishedPath")]
  public string? PublishedPath { get; set; } // e.g., "/online-refiling"

  [JsonPropertyName("pageUrl")]
  public string? PageUrl { get; set; } // Full page URL

  [JsonPropertyName("formId")]
  public string? FormId { get; set; } // Form ID
  [JsonPropertyName("name")]
  public string? Name { get; set; }

  [JsonPropertyName("siteId")]
  public string? SiteId { get; set; }

  [JsonPropertyName("data")]
  public WebflowWebhookDto? Data { get; set; }
}

/// <summary>
/// Actual form data from Webflow
/// </summary>
public class WebflowWebhookDto
{
  [JsonPropertyName("OR-Name")]
  public string? OrName { get; set; }

  [JsonPropertyName("OR-Last-name")]
  public string? OrLastName { get; set; }

  [JsonPropertyName("OR-Phone-number")]
  public string? OrPhoneNumber { get; set; }

  [JsonPropertyName("OR-Medication")]
  public object? OrMedication { get; set; }

  [JsonPropertyName("OR Delivery or Pick up")]
  public string? OrDeliveryOrPickUp { get; set; }

  [JsonPropertyName("OR Tomorrow delivery time")]
  public string? OrTomorrowDeliveryTime { get; set; }

  [JsonPropertyName("OR-note")]
  public string? OrNote { get; set; }

  // Capture any additional fields (e.g., Medication 2, Medication 3, Medication 4, etc.)
  [JsonExtensionData]
  public Dictionary<string, JsonElement>? AdditionalFields { get; set; }
}


