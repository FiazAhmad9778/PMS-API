using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Services.Interfaces;

namespace PMS.API.Application.Services.Implementation;

public class InterFaxService : IInterFaxService
{
  private readonly IConfiguration _configuration;
  private readonly ILogger<InterFaxService> _logger;
  private readonly HttpClient _httpClient;
  private readonly string _apiUrl = "https://rest.interfax.net/outbound";

  public InterFaxService(
    IConfiguration configuration,
    ILogger<InterFaxService> logger,
    IHttpClientFactory httpClientFactory)
  {
    _configuration = configuration;
    _logger = logger;
    _httpClient = httpClientFactory.CreateClient();
    
    // Set up basic authentication for InterFAX
    var username = _configuration["InterFax:Username"] ?? "";
    var password = _configuration["InterFax:Password"] ?? "";
    
    if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
    {
      var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
      _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
    }
  }

  public async Task<(bool success, string? transactionId, string? errorMessage)> SendFaxAsync(
    string faxNumber,
    byte[] pdfBytes,
    string? fileName = null)
  {
    try
    {
      // Validate inputs
      if (pdfBytes == null || pdfBytes.Length == 0)
      {
        _logger.LogError("Cannot send fax: PDF bytes are null or empty");
        return (false, null, "PDF file is empty or null");
      }

      if (string.IsNullOrWhiteSpace(faxNumber))
      {
        _logger.LogError("Cannot send fax: Fax number is empty");
        return (false, null, "Fax number is required");
      }

      // Normalize fax number (remove non-digit characters except +)
      var normalizedFaxNumber = NormalizeFaxNumber(faxNumber);
      
      if (string.IsNullOrWhiteSpace(normalizedFaxNumber))
      {
        _logger.LogError("Cannot send fax: Normalized fax number is empty");
        return (false, null, "Invalid fax number format");
      }
      
      _logger.LogInformation("Attempting to send fax via InterFAX to {FaxNumber}, PDF size: {Size} bytes", 
        normalizedFaxNumber, pdfBytes.Length);

      // InterFAX API endpoint - faxNumber must be a query parameter, not form data
      // According to API docs: POST /outbound/faxes?faxNumber={faxnumber}
      var url = $"{_apiUrl}/faxes?faxNumber={Uri.EscapeDataString(normalizedFaxNumber)}";
      _logger.LogInformation("Sending fax to InterFAX API: {Url}", url);

      // According to InterFAX API documentation:
      // - For single document: Send binary content directly in request body
      // - Content-Type should be application/pdf
      // - Response is 201 Created with Location header containing the transaction ID
      var pdfContent = new ByteArrayContent(pdfBytes);
      pdfContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

      // Create request with PDF as body content
      using var request = new HttpRequestMessage(HttpMethod.Post, url)
      {
        Content = pdfContent
      };

      _logger.LogInformation("Request URI: {RequestUri}, Method: {Method}, Content-Type: {ContentType}", 
        request.RequestUri, request.Method, pdfContent.Headers.ContentType);

      var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
      var responseContent = await response.Content.ReadAsStringAsync();
      
      _logger.LogInformation("InterFAX API Response: StatusCode={StatusCode}, ReasonPhrase={ReasonPhrase}, Location={Location}, Content={Content}", 
        response.StatusCode, response.ReasonPhrase, 
        response.Headers.Location?.ToString() ?? "None", 
        responseContent);

      // According to API docs: On success, returns 201 Created with Location header
      // Location format: https://rest.interfax.net/outbound/faxes/{id}
      if (response.StatusCode == System.Net.HttpStatusCode.Created)
      {
        // Extract transaction ID from Location header
        var locationHeader = response.Headers.Location;
        string? transactionId = null;

        if (locationHeader != null)
        {
          // Location format: https://rest.interfax.net/outbound/faxes/854759652
          var locationPath = locationHeader.AbsolutePath;
          var segments = locationPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
          
          // Find the fax ID (should be the last segment after /faxes/)
          if (segments.Length > 0)
          {
            // Look for the segment after "faxes"
            var faxesIndex = Array.IndexOf(segments, "faxes");
            if (faxesIndex >= 0 && faxesIndex < segments.Length - 1)
          {
              transactionId = segments[faxesIndex + 1];
            }
            else
            {
              // Fallback: use last segment
              transactionId = segments[segments.Length - 1];
          }
          }
        }

        // CRITICAL: Validate that we actually got a transaction ID from the Location header
        // DO NOT generate a fake transaction ID - if we don't have one, it's a failure
        if (string.IsNullOrWhiteSpace(transactionId))
        {
          _logger.LogError("InterFAX API returned 201 Created but no transaction ID in Location header. Location: {Location}, Response: {Response}", 
            locationHeader?.ToString() ?? "None", responseContent);
          return (false, null, "InterFAX API returned 201 Created but no transaction ID found in Location header");
        }

        _logger.LogInformation("Fax queued successfully via InterFAX. TransactionId: {TransactionId}", transactionId);
        return (true, transactionId, null);
          }
      else if (response.IsSuccessStatusCode)
        {
        // Unexpected success status (not 201) - this shouldn't happen per API docs
        _logger.LogWarning("InterFAX API returned unexpected success status: {StatusCode} (expected 201 Created). Response: {Response}", 
          response.StatusCode, responseContent);
        
        // Try to extract ID from Location header anyway
        var locationHeader = response.Headers.Location;
        if (locationHeader != null)
        {
          var locationPath = locationHeader.AbsolutePath;
          var segments = locationPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
          if (segments.Length > 0)
          {
            var faxesIndex = Array.IndexOf(segments, "faxes");
            if (faxesIndex >= 0 && faxesIndex < segments.Length - 1)
            {
              var transactionId = segments[faxesIndex + 1];
              if (!string.IsNullOrWhiteSpace(transactionId))
              {
                _logger.LogInformation("Extracted transaction ID from Location header: {TransactionId}", transactionId);
        return (true, transactionId, null);
              }
            }
          }
        }
        
        return (false, null, $"InterFAX API returned unexpected status: {response.StatusCode}");
      }
      else
      {
        // HTTP error status code
        _logger.LogError("InterFAX API error: {StatusCode} - {Response}", response.StatusCode, responseContent);
        return (false, null, $"InterFAX API error: {response.StatusCode} - {responseContent}");
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error sending fax via InterFAX to {FaxNumber}", faxNumber);
      return (false, null, $"Error: {ex.Message}");
    }
  }

  public async Task<(bool success, string? status, string? errorMessage)> GetFaxStatusAsync(string transactionId)
  {
    try
    {
      var url = $"{_apiUrl}/faxes/{transactionId}";
      var response = await _httpClient.GetAsync(url);
      var responseContent = await response.Content.ReadAsStringAsync();

      if (response.IsSuccessStatusCode)
      {
        try
        {
          var jsonResponse = JsonDocument.Parse(responseContent);
          string? status = null;
          
          if (jsonResponse.RootElement.TryGetProperty("status", out var statusElement))
          {
            status = statusElement.GetString();
          }
          else if (jsonResponse.RootElement.TryGetProperty("state", out var stateElement))
          {
            status = stateElement.GetString();
          }

          return (true, status ?? "Unknown", null);
        }
        catch
        {
          return (true, "Unknown", null);
        }
      }
      else
      {
        _logger.LogError("InterFAX status check error: {StatusCode} - {Response}", response.StatusCode, responseContent);
        return (false, null, $"InterFAX API error: {response.StatusCode}");
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error checking fax status for TransactionId: {TransactionId}", transactionId);
      return (false, null, $"Error: {ex.Message}");
    }
  }

  private string NormalizeFaxNumber(string faxNumber)
  {
    // Remove all non-digit characters except +
    var normalized = new StringBuilder();
    foreach (var c in faxNumber)
    {
      if (char.IsDigit(c) || c == '+')
      {
        normalized.Append(c);
      }
    }
    return normalized.ToString();
  }
}





