using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Orders.Commands.CreateOrderFromWebhook;
using PMS.API.Application.Features.Orders.DTOs;
using PMS.API.Application.Services.Interfaces;
using PMS.API.Core.Domain.Entities;

namespace PMS.API.Web.Api;

[Route("api/[controller]")]
[ApiController]
public class WebhookController : BaseApiController
{
  private readonly ILogger<WebhookController> _logger;
  private readonly IConfiguration _configuration;

  public WebhookController(IServiceProvider serviceProvider, ILogger<WebhookController> logger, IConfiguration configuration) : base(serviceProvider)
  {
    _logger = logger;
    _configuration = configuration;
  }

  [HttpPost("webflow")]
  [AllowAnonymous]
  [ProducesResponseType(typeof(ApplicationResult<long>), StatusCodes.Status200OK)]
  public async Task<IActionResult> ReceiveWebflowWebhook()
  {
    try
    {
      // Read raw body once (will be reused for signature validation and deserialization)
      Request.EnableBuffering();
      Request.Body.Position = 0;
      using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
      var rawBody = await reader.ReadToEndAsync();
      Request.Body.Position = 0;

      _logger.LogInformation("Webhook raw body: {Body}", rawBody);

      // Validate signature header
      var secret = _configuration["Webflow:WebhookSecret"];
      if (!string.IsNullOrEmpty(secret))
      {
        // Get timestamp header
        var timestampHeader = Request.Headers["x-webflow-timestamp"].FirstOrDefault();
        if (string.IsNullOrEmpty(timestampHeader))
        {
          _logger.LogWarning("Missing x-webflow-timestamp header");
          return Unauthorized(new { error = "Missing timestamp header" });
        }

        // Verify timestamp is within 5 minutes to prevent replay attacks
        if (!long.TryParse(timestampHeader, out var timestamp))
        {
          _logger.LogWarning("Invalid timestamp format: {Timestamp}", timestampHeader);
          return Unauthorized(new { error = "Invalid timestamp format" });
        }

        // Webflow sends timestamps in milliseconds
        // Validate timestamp is within 5 minutes (300000 milliseconds)
        var currentTimeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var timeDifferenceMs = currentTimeMs - timestamp;

        if (Math.Abs(timeDifferenceMs) > 300000) // 5 minutes in milliseconds
        {
          _logger.LogWarning("Request timestamp is too old or too far in future. Request timestamp: {RequestTimestamp}, Current timestamp: {CurrentTimestamp}, Difference: {Difference} ms",
            timestamp, currentTimeMs, timeDifferenceMs);
          return Unauthorized(new { error = "Request timestamp is outside acceptable range (5 minutes)" });
        }

        // Get signature header
        var receivedSignature = Request.Headers["x-webflow-signature"].FirstOrDefault();
        if (string.IsNullOrEmpty(receivedSignature))
        {
          _logger.LogWarning("Missing x-webflow-signature header");
          return Unauthorized(new { error = "Missing signature header" });
        }

        // Generate HMAC: timestamp:requestBody (format: timestamp:body)
        var messageToSign = $"{timestampHeader}:{rawBody}";
        var computedSignature = ComputeHmacSha256Hex(messageToSign, secret);

        if (!string.Equals(receivedSignature, computedSignature, StringComparison.OrdinalIgnoreCase))
        {
          _logger.LogWarning("Invalid webhook signature. Expected: {Expected}, Received: {Received}",
            computedSignature, receivedSignature);
          return Unauthorized(new { error = "Invalid signature" });
        }

        _logger.LogInformation("Webhook signature and timestamp validated successfully. Request time: {RequestTime}", 1);
      }

      // Deserialize the root webhook structure (single object with payload.data)
      WebflowWebhookRootDto? webhookRoot = null;
      try
      {
        webhookRoot = JsonSerializer.Deserialize<WebflowWebhookRootDto>(rawBody, new JsonSerializerOptions
        {
          PropertyNameCaseInsensitive = true
        });
      }
      catch (JsonException ex)
      {
        _logger.LogError(ex, "Failed to deserialize webhook root structure");
        return BadRequest(new { error = "Invalid JSON format", details = ex.Message });
      }

      if (webhookRoot == null)
      {
        _logger.LogWarning("Webhook root is null");
        return BadRequest(new { error = "Webhook payload is null" });
      }

      // Log webhook metadata
      _logger.LogInformation("Webhook received - PublishedPath: {PublishedPath}, PageUrl: {PageUrl}, FormId: {FormId}, WebhookId: {WebhookId}",
        webhookRoot.Payload?.PublishedPath, webhookRoot.Payload?.PageUrl, webhookRoot.Payload?.FormId, webhookRoot.Payload?.Id);

      // Extract form data from payload.data
      var formData = webhookRoot.Payload?.Data;

      if (formData == null)
      {
        _logger.LogWarning("Form data is null in webhook payload");
        return BadRequest(new { error = "Form data not found in payload" });
      }

      // Check for duplicate webhook (idempotency check) - check BEFORE processing any form type
      var webhookId = webhookRoot.Payload?.Id;
      if (!string.IsNullOrEmpty(webhookId))
      {
        var orderRepository = HttpContext.RequestServices.GetRequiredService<PMS.API.Core.Domain.Interfaces.Repositories.IOrderRepository>();
        var existingOrder = await orderRepository.GetByWebhookIdAsync(webhookId);

        if (existingOrder != null)
        {
          _logger.LogInformation("Webhook {WebhookId} already processed. Returning existing OrderId: {OrderId}",
            webhookId, existingOrder.Id);
          return Ok(ApplicationResult<long>.SuccessResult(existingOrder.Id, "Webhook already processed (duplicate)"));
        }
      }

      // Check if this is a Contact Form submission - MUST check BEFORE order processing
      var formName = webhookRoot.Payload?.Name;
      var publishedPath = webhookRoot.Payload?.PublishedPath;
      
      // Check form name and also check if it's a contact page
      var isContactForm = (!string.IsNullOrEmpty(formName) && 
                          formName.Contains("Contact", StringComparison.OrdinalIgnoreCase)) ||
                         (!string.IsNullOrEmpty(publishedPath) && 
                          publishedPath.Contains("contact", StringComparison.OrdinalIgnoreCase));

      // Detect transfer request / become-a-patient form
      // Webflow form name may be generic ("Email Form"), so also detect by page path and field keys.
      var isTransferRequestForm =
        (!string.IsNullOrEmpty(formName) &&
         (formName.Contains("Email Form", StringComparison.OrdinalIgnoreCase) ||
          formName.Contains("Transfer", StringComparison.OrdinalIgnoreCase))) ||
        (!string.IsNullOrEmpty(publishedPath) &&
         (publishedPath.Contains("get-started", StringComparison.OrdinalIgnoreCase) ||
          publishedPath.Contains("become", StringComparison.OrdinalIgnoreCase) ||
          publishedPath.Contains("patient", StringComparison.OrdinalIgnoreCase)));

      _logger.LogInformation("Form detection - FormName: {FormName}, PublishedPath: {PublishedPath}, IsContactForm: {IsContactForm}",
        formName, publishedPath, isContactForm);

      if (isContactForm)
      {
        _logger.LogInformation("Processing Contact Form submission. Form name: {FormName}", formName);
        
        // Extract Contact Form fields from AdditionalFields
        // All Contact Form fields will be in AdditionalFields since they don't match the OR-* properties
        string? contactName = null;
        string? contactLastName = null;
        string? contactPhone = null;
        string? contactEmail = null;
        string? notes = null;

        if (formData.AdditionalFields != null)
        {
          _logger.LogInformation("AdditionalFields count: {Count}, Keys: {Keys}", 
            formData.AdditionalFields.Count, string.Join(", ", formData.AdditionalFields.Keys));

          foreach (var field in formData.AdditionalFields)
          {
            var fieldName = field.Key;
            var fieldValue = field.Value.ToString();

            // Match field names case-insensitively
            if (fieldName.Equals("Contact name", StringComparison.OrdinalIgnoreCase))
            {
              contactName = fieldValue;
            }
            else if (fieldName.Equals("Contact last name", StringComparison.OrdinalIgnoreCase))
            {
              contactLastName = fieldValue;
            }
            else if (fieldName.Equals("Contact phone", StringComparison.OrdinalIgnoreCase))
            {
              contactPhone = fieldValue;
            }
            else if (fieldName.Equals("Contact email", StringComparison.OrdinalIgnoreCase))
            {
              contactEmail = fieldValue;
            }
            else if (fieldName.Equals("Field", StringComparison.OrdinalIgnoreCase) || 
                     fieldName.Contains("note", StringComparison.OrdinalIgnoreCase) ||
                     fieldName.Contains("message", StringComparison.OrdinalIgnoreCase))
            {
              notes = fieldValue;
            }
          }
        }

        // If fields not found in AdditionalFields, try parsing from raw JSON
        if (string.IsNullOrEmpty(contactName) || string.IsNullOrEmpty(contactLastName) || 
            string.IsNullOrEmpty(contactPhone) || string.IsNullOrEmpty(contactEmail))
        {
          _logger.LogInformation("Contact fields not found in AdditionalFields, attempting to parse from raw JSON");
          
          try
          {
            // Parse the data section directly from raw body
            var dataJson = JsonSerializer.Deserialize<JsonElement>(rawBody);
            if (dataJson.TryGetProperty("payload", out var payload) && 
                payload.TryGetProperty("data", out var data))
            {
              if (data.TryGetProperty("Contact name", out var nameEl))
                contactName = nameEl.GetString();
              if (data.TryGetProperty("Contact last name", out var lastNameEl))
                contactLastName = lastNameEl.GetString();
              if (data.TryGetProperty("Contact phone", out var phoneEl))
                contactPhone = phoneEl.GetString();
              if (data.TryGetProperty("Contact email", out var emailEl))
                contactEmail = emailEl.GetString();
              if (data.TryGetProperty("Field", out var fieldEl))
                notes = fieldEl.GetString();
            }
          }
          catch (Exception ex)
          {
            _logger.LogWarning(ex, "Failed to parse contact fields from raw JSON");
          }
        }

        if (string.IsNullOrEmpty(contactName) || string.IsNullOrEmpty(contactLastName) || 
            string.IsNullOrEmpty(contactPhone) || string.IsNullOrEmpty(contactEmail))
        {
          _logger.LogWarning("Missing required Contact Form fields. Name: {Name}, LastName: {LastName}, Phone: {Phone}, Email: {Email}. Raw body logged above.",
            contactName, contactLastName, contactPhone, contactEmail);
          return BadRequest(new { error = "Missing required Contact Form fields", 
            details = $"Name: {contactName}, LastName: {contactLastName}, Phone: {contactPhone}, Email: {contactEmail}" });
        }

        // Check if this contact form webhook has already been processed (idempotency)
        if (!string.IsNullOrEmpty(webhookId))
        {
          var orderRepository = HttpContext.RequestServices.GetRequiredService<PMS.API.Core.Domain.Interfaces.Repositories.IOrderRepository>();
          var existingOrder = await orderRepository.GetByWebhookIdAsync(webhookId);

          if (existingOrder != null)
          {
            _logger.LogInformation("Contact form webhook {WebhookId} already processed. Skipping duplicate submission.",
              webhookId);
            return Ok(new { success = true, message = "Contact form already processed (duplicate webhook)", webhookId });
          }
        }

        // Generate PDF and send fax
        var pdfService = HttpContext.RequestServices.GetRequiredService<IPdfService>();
        var interFaxService = HttpContext.RequestServices.GetRequiredService<IInterFaxService>();
        var faxNumber = _configuration["InterFax:DefaultFaxNumber"];

        if (string.IsNullOrEmpty(faxNumber))
        {
          _logger.LogError("Fax number not configured");
          return BadRequest(new { error = "Fax number not configured" });
        }

        try
        {
          var pdfBytes = await pdfService.GenerateContactFormPdfAsync(
            contactName, contactLastName, contactPhone, contactEmail, notes);

          var (success, transactionId, errorMessage) = await interFaxService.SendFaxAsync(
            faxNumber, pdfBytes, $"contact_form_{webhookId}.pdf");

          if (success)
          {
            // Store webhookId to prevent duplicate processing
            // Create a minimal order record to track this webhook (for idempotency)
            if (!string.IsNullOrEmpty(webhookId))
            {
              try
              {
                var orderRepository = HttpContext.RequestServices.GetRequiredService<PMS.API.Core.Domain.Interfaces.Repositories.IOrderRepository>();
                var trackingOrder = new PMS.API.Core.Domain.Entities.Order
                {
                  FirstName = contactName,
                  LastName = contactLastName,
                  PhoneNumber = contactPhone,
                  Medication = "[]", // Empty for contact forms
                  DeliveryOrPickup = "Contact Form",
                  FaxStatus = "Sent", // Mark as sent since we just sent it
                  FaxTransactionId = transactionId,
                  FaxSentAt = DateTime.UtcNow,
                  WebhookId = webhookId,
                  Notes = $"Contact Form - Email: {contactEmail}"
                };
                await orderRepository.AddAsync(trackingOrder);
                _logger.LogInformation("Contact form webhook {WebhookId} tracked in database for idempotency", webhookId);
              }
              catch (Exception trackEx)
              {
                // Log but don't fail - the fax was sent successfully
                _logger.LogWarning(trackEx, "Failed to track contact form webhook {WebhookId} in database", webhookId);
              }
            }

            _logger.LogInformation("Contact form PDF sent via fax successfully. TransactionId: {TransactionId}, WebhookId: {WebhookId}",
              transactionId, webhookId);
            return Ok(new { success = true, message = "Contact form submitted and faxed successfully", transactionId, webhookId });
          }
          else
          {
            _logger.LogError("Failed to send contact form via fax. Error: {Error}", errorMessage);
            return StatusCode(500, new { error = "Failed to send fax", details = errorMessage });
          }
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Error processing contact form submission");
          return StatusCode(500, new { error = "Error processing contact form", details = ex.Message });
        }
      }
      else if (isTransferRequestForm)
      {
        _logger.LogInformation("Processing Transfer Request submission. Form name: {FormName}, PublishedPath: {PublishedPath}", formName, publishedPath);

        // Idempotency: if webhook already stored (tracking record), skip duplicate fax
        if (!string.IsNullOrEmpty(webhookId))
        {
          var orderRepository = HttpContext.RequestServices.GetRequiredService<PMS.API.Core.Domain.Interfaces.Repositories.IOrderRepository>();
          var existingOrder = await orderRepository.GetByWebhookIdAsync(webhookId);
          if (existingOrder != null)
          {
            _logger.LogInformation("Transfer request webhook {WebhookId} already processed. Skipping duplicate submission.", webhookId);
            return Ok(new { success = true, message = "Transfer request already processed (duplicate webhook)", webhookId });
          }
        }

        // Try extract fields from AdditionalFields first, then raw JSON fallback
        var additional = formData.AdditionalFields;

        string? firstName = GetField(additional, "Form first name", "First name", "First Name");
        string? lastName = GetField(additional, "Form last name", "Last name", "Last Name");
        string? phone1 = GetField(additional, "Form phone number", "Phone number", "Phone Number");
        string? phone2 = GetField(additional, "Form Phone Number 2", "Phone Number 2", "Phone number 2");
        string? dob = GetField(additional, "Date of Birth", "DOB", "Form date of birth", "Form DOB");
        string? transferringFrom = GetField(additional, "Form transfer", "Pharmacy they are transferring from", "Transferring from", "Transfer");
        string? notes = GetField(additional, "Form area", "Any special questions or concerns", "Notes", "Note", "Field");

        // Raw JSON fallback (payload.data has the exact field names)
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(phone1))
        {
          try
          {
            var root = JsonSerializer.Deserialize<JsonElement>(rawBody);
            if (root.TryGetProperty("payload", out var payload) && payload.TryGetProperty("data", out var data))
            {
              firstName ??= GetField(data, "Form first name", "First name", "First Name");
              lastName ??= GetField(data, "Form last name", "Last name", "Last Name");
              phone1 ??= GetField(data, "Form phone number", "Phone number", "Phone Number");
              phone2 ??= GetField(data, "Form Phone Number 2", "Phone Number 2", "Phone number 2");
              dob ??= GetField(data, "Date of Birth", "DOB", "Form date of birth", "Form DOB");
              transferringFrom ??= GetField(data, "Form transfer", "Pharmacy they are transferring from", "Transferring from", "Transfer");
              notes ??= GetField(data, "Form area", "Any special questions or concerns", "Notes", "Note", "Field");
            }
          }
          catch (Exception ex)
          {
            _logger.LogWarning(ex, "Failed to parse transfer request fields from raw JSON");
          }
        }

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(phone1))
        {
          _logger.LogWarning("Missing required transfer request fields. FirstName: {FirstName}, LastName: {LastName}, Phone: {Phone}",
            firstName, lastName, phone1);
          return BadRequest(new { error = "Missing required transfer request fields" });
        }

        var pdfService = HttpContext.RequestServices.GetRequiredService<IPdfService>();
        var interFaxService = HttpContext.RequestServices.GetRequiredService<IInterFaxService>();
        var faxNumber = _configuration["InterFax:DefaultFaxNumber"];
        if (string.IsNullOrEmpty(faxNumber))
        {
          _logger.LogError("Fax number not configured");
          return BadRequest(new { error = "Fax number not configured" });
        }

        try
        {
          var pdfBytes = await pdfService.GenerateTransferRequestPdfAsync(
            firstName, lastName, phone1, phone2, dob, transferringFrom, notes);

          var (success, transactionId, errorMessage) = await interFaxService.SendFaxAsync(
            faxNumber, pdfBytes, $"transfer_request_{webhookId}.pdf");

          if (!success)
          {
            _logger.LogError("Failed to send transfer request via fax. Error: {Error}", errorMessage);
            return StatusCode(500, new { error = "Failed to send fax", details = errorMessage });
          }

          // Track webhookId in DB for idempotency
          if (!string.IsNullOrEmpty(webhookId))
          {
            try
            {
              var orderRepository = HttpContext.RequestServices.GetRequiredService<PMS.API.Core.Domain.Interfaces.Repositories.IOrderRepository>();
              var trackingOrder = new Order
              {
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phone1,
                Medication = "[]",
                DeliveryOrPickup = "Transfer Request",
                Address = transferringFrom,
                Notes = $"DOB: {dob}\nPhone2: {phone2}\nNotes: {notes}",
                FaxStatus = "Sent",
                FaxTransactionId = transactionId,
                FaxSentAt = DateTime.UtcNow,
                WebhookId = webhookId
              };
              await orderRepository.AddAsync(trackingOrder);
              _logger.LogInformation("Transfer request webhook {WebhookId} tracked in database for idempotency", webhookId);
            }
            catch (Exception trackEx)
            {
              _logger.LogWarning(trackEx, "Failed to track transfer request webhook {WebhookId} in database", webhookId);
            }
          }

          _logger.LogInformation("Transfer request PDF sent via fax successfully. TransactionId: {TransactionId}, WebhookId: {WebhookId}",
            transactionId, webhookId);
          return Ok(new { success = true, message = "Transfer request faxed successfully", transactionId, webhookId });
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Error processing transfer request submission");
          return StatusCode(500, new { error = "Error processing transfer request", details = ex.Message });
        }
      }
      else
      {
        // Handle regular order form
        _logger.LogInformation("Parsed webflow data - Name: {Name}, LastName: {LastName}, Phone: {Phone}, Delivery: {Delivery}",
          formData.OrName, formData.OrLastName, formData.OrPhoneNumber, formData.OrDeliveryOrPickUp);

        var command = new CreateOrderFromWebhookCommand
        {
          WebflowData = formData,
          WebhookId = webhookId
        };

        var result = await Mediator.Send(command);
        if (result.IsSuccess)
        {
          _logger.LogInformation("Order created successfully from webhook. OrderId: {OrderId}, WebhookId: {WebhookId}",
            result.Data, webhookId);
          return Ok(result);
        }

        _logger.LogWarning("Failed to create order from webhook. Error: {Error}", result.Message);
        return BadRequest(result);
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error processing webhook");
      return StatusCode(500, new { error = "Internal server error", details = ex.Message });
    }
  }

  private static string ComputeHmacSha256Hex(string payload, string secret)
  {
    var keyBytes = Encoding.UTF8.GetBytes(secret);
    var payloadBytes = Encoding.UTF8.GetBytes(payload);
    using var hmac = new HMACSHA256(keyBytes);
    var hash = hmac.ComputeHash(payloadBytes);
    return Convert.ToHexString(hash).ToLowerInvariant();
  }

  private static string? GetField(Dictionary<string, JsonElement>? additionalFields, params string[] keys)
  {
    if (additionalFields == null) return null;

    foreach (var key in keys)
    {
      if (additionalFields.TryGetValue(key, out var value))
        return value.ToString();
    }

    foreach (var key in keys)
    {
      foreach (var kv in additionalFields)
      {
        if (kv.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
          return kv.Value.ToString();
      }
    }

    return null;
  }

  private static string? GetField(JsonElement data, params string[] keys)
  {
    foreach (var key in keys)
    {
      if (data.TryGetProperty(key, out var value))
        return value.ToString();
    }

    return null;
  }
}


