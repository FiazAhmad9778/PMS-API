using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Orders.DTOs;
using PMS.API.Core.Domain.Entities;
using PMS.API.Core.Domain.Interfaces.Repositories;

namespace PMS.API.Application.Features.Orders.Commands.CreateOrderFromWebhook;

public class CreateOrderFromWebhookCommand : IRequest<ApplicationResult<long>>
{
  public required WebflowWebhookDto WebflowData { get; set; }
  public string? WebhookId { get; set; } // Unique webhook ID for idempotency
}

public class CreateOrderFromWebhookCommandHandler : RequestHandlerBase<CreateOrderFromWebhookCommand, ApplicationResult<long>>
{
  private readonly IOrderRepository _orderRepository;

  public CreateOrderFromWebhookCommandHandler(
    IOrderRepository orderRepository,
    IServiceProvider serviceProvider,
    ILogger<CreateOrderFromWebhookCommandHandler> logger) : base(serviceProvider, logger)
  {
    _orderRepository = orderRepository;
  }

  protected override async Task<ApplicationResult<long>> HandleRequest(
    CreateOrderFromWebhookCommand request,
    CancellationToken cancellationToken)
  {
    try
    {
      var webflowData = request.WebflowData;

      // Parse medications (can be string, array, or JSON)
      var medicationsJson = ParseMedications(webflowData);

      // Extract fields - check both direct properties and AdditionalFields
      var firstName = webflowData.OrName ?? ExtractFromAdditionalFields(webflowData, "OR-Name", "OR Name", "Name");
      var lastName = webflowData.OrLastName ?? ExtractFromAdditionalFields(webflowData, "OR-Last-name", "OR Last name", "Last name", "LastName");
      var phoneNumber = webflowData.OrPhoneNumber ?? ExtractFromAdditionalFields(webflowData, "OR-Phone-number", "OR Phone number", "Phone number", "PhoneNumber", "Phone");
      var deliveryOrPickup = webflowData.OrDeliveryOrPickUp ?? ExtractFromAdditionalFields(webflowData, "OR Delivery or Pick up", "Delivery or Pick up", "DeliveryOrPickup");
      var deliveryTimeSlot = webflowData.OrTomorrowDeliveryTime ?? ExtractFromAdditionalFields(webflowData, "OR Tomorrow delivery time", "Tomorrow delivery time", "DeliveryTimeSlot");
      var address = ExtractFromAdditionalFields(webflowData, "OR-Address", "OR Address", "Address", "address");
      var notes = webflowData.OrNote ?? ExtractFromAdditionalFields(webflowData, "OR-note", "OR note", "Note", "Notes");

      Logger.LogInformation("Extracted order data - FirstName: {FirstName}, LastName: {LastName}, Phone: {Phone}, Delivery: {Delivery}, Address: {Address}",
        firstName, lastName, phoneNumber, deliveryOrPickup, address);

      // Map Webflow data to Order entity
      var order = new Order
      {
        FirstName = !string.IsNullOrWhiteSpace(firstName) ? firstName : "Unknown",
        LastName = !string.IsNullOrWhiteSpace(lastName) ? lastName : "Unknown",
        PhoneNumber = phoneNumber ?? "",
        Medication = medicationsJson,
        DeliveryOrPickup = deliveryOrPickup ?? "",
        Address = address,
        DeliveryTimeSlot = deliveryTimeSlot,
        Notes = notes,
        FaxStatus = "Pending",
        FaxRetryCount = 0,
        WebhookId = request.WebhookId // Store webhook ID for idempotency
      };

      // Save order to database
      var savedOrder = await _orderRepository.AddAsync(order);

      Logger.LogInformation("Order created with ID: {OrderId}. Will be processed by recurring job.", savedOrder.Id);

      return ApplicationResult<long>.SuccessResult(savedOrder.Id, "Order created successfully");
    }
    catch (Exception ex)
    {
      Logger.LogError(ex, "Error creating order from webhook");
      return ApplicationResult<long>.Error($"Error creating order: {ex.Message}");
    }
  }

  private string ParseMedications(WebflowWebhookDto webflowData)
  {
    try
    {
      var sources = new List<object?>
      {
        webflowData.OrMedication
      };

      // Add dynamic medication fields captured via JsonExtensionData (Medication 4, Medication 5, ...)
      sources.AddRange(GetAdditionalMedicationValues(webflowData));

      var allMedications = new List<string>();

      foreach (var source in sources)
      {
        if (source == null) continue;

        switch (source)
        {
          case List<string> list:
            allMedications.AddRange(list);
            break;
          case JsonElement jsonElement:
            if (jsonElement.ValueKind == JsonValueKind.Array)
            {
              allMedications.AddRange(jsonElement
                .EnumerateArray()
                .Select(x => x.GetString() ?? "")
                .Where(x => !string.IsNullOrWhiteSpace(x)));
            }
            else if (jsonElement.ValueKind == JsonValueKind.String)
            {
              var str = jsonElement.GetString();
              if (!string.IsNullOrWhiteSpace(str))
              {
                try
                {
                  var parsed = JsonSerializer.Deserialize<List<string>>(str);
                  if (parsed != null)
                  {
                    allMedications.AddRange(parsed);
                  }
                }
                catch
                {
                  allMedications.Add(str);
                }
              }
            }
            break;
          case string medicationString:
            if (string.IsNullOrWhiteSpace(medicationString)) break;
            try
            {
              var parsed = JsonSerializer.Deserialize<List<string>>(medicationString);
              if (parsed != null)
              {
                allMedications.AddRange(parsed);
                break;
              }
            }
            catch
            {
              // ignore and fallback
            }

            var split = medicationString.Split(',', StringSplitOptions.RemoveEmptyEntries)
              .Select(m => m.Trim())
              .Where(m => !string.IsNullOrWhiteSpace(m))
              .ToList();

            if (split.Any())
            {
              allMedications.AddRange(split);
            }
            else
            {
              allMedications.Add(medicationString.Trim());
            }
            break;
          default:
            var strValue = GetStringValue(source);
            if (!string.IsNullOrWhiteSpace(strValue))
            {
              allMedications.Add(strValue);
            }
            break;
        }
      }

      if (!allMedications.Any())
      {
        Logger.LogWarning("No medication source found in webflow data");
      }

      var collected = CollectMedicationStrings(webflowData);
      allMedications.AddRange(collected);

      var distinct = allMedications
        .Where(x => !string.IsNullOrWhiteSpace(x))
        .Select(x => x.Trim())
        .Distinct()
        .ToList();

      return JsonSerializer.Serialize(distinct);
    }
    catch (Exception ex)
    {
      Logger.LogWarning(ex, "Error parsing medications, using empty list");
      return JsonSerializer.Serialize(new List<string>());
    }
  }

  private List<string> CollectMedicationStrings(WebflowWebhookDto webflowData)
  {
    var meds = new List<string?>();
    meds.Add(GetStringValue(webflowData.OrMedication));
    meds.AddRange(GetAdditionalMedicationValues(webflowData).Select(GetStringValue));

    return meds
      .Where(x => !string.IsNullOrWhiteSpace(x))
      .Select(x => x!.Trim())
      .Distinct()
      .ToList();
  }

  private string? GetStringValue(object? value)
  {
    switch (value)
    {
      case null:
        return null;
      case string str:
        return str;
      case JsonElement element when element.ValueKind == JsonValueKind.String:
        return element.GetString();
      default:
        return value.ToString();
    }
  }

  private IEnumerable<object?> GetAdditionalMedicationValues(WebflowWebhookDto webflowData)
  {
    if (webflowData.AdditionalFields == null)
    {
      return Enumerable.Empty<object?>();
    }

    return webflowData.AdditionalFields
      .Where(kv => kv.Key.Contains("medication", StringComparison.OrdinalIgnoreCase))
      .Select(kv => (object?)kv.Value);
  }

  private string? ExtractFromAdditionalFields(WebflowWebhookDto webflowData, params string[] fieldNames)
  {
    if (webflowData.AdditionalFields == null)
    {
      return null;
    }

    foreach (var fieldName in fieldNames)
    {
      if (webflowData.AdditionalFields.TryGetValue(fieldName, out var value))
      {
        return value.ToString();
      }
    }

    // Try case-insensitive match
    foreach (var fieldName in fieldNames)
    {
      var match = webflowData.AdditionalFields.FirstOrDefault(kv => 
        kv.Key.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
      if (match.Key != null)
      {
        return match.Value.ToString();
      }
    }

    return null;
  }
}

