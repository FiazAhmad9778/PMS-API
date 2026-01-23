using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Services.Interfaces;
using PMS.API.Core.Domain.Entities;
using PMS.API.Core.Domain.Interfaces.Repositories;
using PMS.API.Core.Enums;
using PMS.API.Infrastructure.Data;
using UglyToad.PdfPig;

namespace PMS.API.Application.Services.Implementation;

public class DocumentService : IDocumentService
{
  private readonly IConfiguration _configuration;
  private readonly IDocumentRepository _documentRepository;
  private readonly IServiceScopeFactory _serviceScopeFactory;
  private readonly string _baseUrl;
  private readonly string _username;
  private readonly string _password;

  public DocumentService(
    IConfiguration configuration,
    IDocumentRepository documentRepository,
    IServiceScopeFactory serviceScopeFactory)
  {
    _configuration = configuration;
    _documentRepository = documentRepository;
    _serviceScopeFactory = serviceScopeFactory;
    _baseUrl = _configuration["Document:BaseUrl"] ?? throw new ArgumentNullException("Base URL is missing in configuration.");
    _username = _configuration["Document:Username"] ?? throw new ArgumentNullException("Username is missing in configuration.");
    _password = _configuration["Document:Password"] ?? throw new ArgumentNullException("Password is missing in configuration.");
  }

  public async Task<FileContentResult> GetDocumentPdfAsync(long documentId)
  {
    var document = await _documentRepository.Get().FirstOrDefaultAsync(d => d.Id == documentId) ?? throw new Exception("Document not found.");
    var byteArray = await DownloadPdf(document.DocumentUrl);
    return new FileContentResult(byteArray, "application/pdf")
    {
      FileDownloadName = document.DocumentName
    };
  }

  public async Task SyncDocuments(CancellationToken stoppingToken)
  {
    try
    {
      var documentNames = await FetchDocumentNames("/pending/");
      foreach (var documentName in documentNames)
      {
        await ProcessAndSaveDocument(documentName);
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine($"[ERROR] SyncDocuments: {ex.Message}");
    }
  }

  private async Task<List<string>> FetchDocumentNames(string folderPath)
  {
    using HttpClient client = new HttpClient();

    string authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_username}:{_password}"));
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

    var response = await client.GetAsync($"{_baseUrl}{folderPath}");

    if (!response.IsSuccessStatusCode)
      throw new Exception($"Failed to fetch document list. Status: {response.StatusCode}");

    var htmlContent = await response.Content.ReadAsStringAsync();
    return ExtractFileNamesFromHtml(htmlContent, ".pdf", 1);
  }

  private static List<string> ExtractFileNamesFromHtml(string htmlContent, string fileExtension, int minAgeMinutes = 1)
  {
    var documentNames = new List<string>();
    var now = DateTime.UtcNow;

    var lines = htmlContent.Split('\n');
    var regex = new Regex(@"href=""(?<href>[^""]+\.pdf)""[^>]*>(?<name>.*?)<\/a>\s+(?<date>\d{2}-[A-Za-z]{3}-\d{4})\s+(?<time>\d{2}:\d{2})");

    foreach (var line in lines)
    {
      var match = regex.Match(line);
      if (!match.Success) continue;

      var fileName = WebUtility.UrlDecode(match.Groups["href"].Value.Trim());
      var dateStr = match.Groups["date"].Value;
      var timeStr = match.Groups["time"].Value;

      if (DateTime.TryParseExact($"{dateStr} {timeStr}", "dd-MMM-yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var uploadTime))
      {
        if ((now - uploadTime).TotalMinutes >= minAgeMinutes)
        {
          documentNames.Add(Path.GetFileName(fileName));
        }
      }
    }

    return documentNames;
  }

  private async Task ProcessAndSaveDocument(string documentName)
  {
    try
    {
      var encodedName = Uri.EscapeDataString(documentName);
      var metadata = await GetDocumentMetadata(encodedName);
      var newDocumentUrl = await MoveDocument(encodedName, true);
      await SaveDocument(newDocumentUrl, documentName, metadata, DocumentStatus.DataEntryRegTech);
      Console.WriteLine($"[SUCCESS] Processed document: {documentName}");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"[ERROR] Processing failed for {documentName}: {ex.Message}");
      var newDocumentUrl = await MoveDocument(documentName);
      await SaveDocument(newDocumentUrl, documentName, new Dictionary<string, string>(), DocumentStatus.Failed, ex.Message);
    }
  }

  private async Task<Dictionary<string, string>> GetDocumentMetadata(string documentName)
  {
    var pdfBytes = await DownloadPdf($"{_baseUrl}/pending/{documentName}");
    return ExtractDocumentMetadata(pdfBytes);
  }

  private Dictionary<string, string> ExtractDocumentMetadata(byte[] pdfBytes)
  {
    using var document = PdfDocument.Open(pdfBytes);
    var uniqueNames = new HashSet<string>();
    var rxNumbers = new HashSet<string>();
    var cycles = new HashSet<string>();

    foreach (var page in document.GetPages())
    {
      var text = Regex.Replace(page.Text, @"\s+", " ");
      var nameMatch = Regex.Match(text, @"[A-Z][a-z]+, [A-Z][a-z]+");

      if (nameMatch.Success)
        uniqueNames.Add(nameMatch.Value.Trim());

      cycles.UnionWith(ExtractCycles(text));
      rxNumbers.UnionWith(ExtractRxNumbers(text));
    }

    return new Dictionary<string, string>
        {
            { "UniqueNames", string.Join(",", uniqueNames) },
            { "Cycles", string.Join(",", cycles) },
            { "RXNumbers", string.Join(",", rxNumbers) }
        };
  }

  private async Task<byte[]> DownloadPdf(string url)
  {
    // Create HttpClient with the custom handler
    using HttpClient client = new HttpClient();

    // Add Basic Authentication header manually
    string authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_username}:{_password}"));
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

    var response = await client.GetAsync(url);

    if (!response.IsSuccessStatusCode)
      throw new Exception($"Failed to download the PDF. Status: {response.StatusCode}");

    return await response.Content.ReadAsByteArrayAsync();
  }

  private List<string> ExtractCycles(string text)
  {
    var cycles = new List<string>();
    var matches = Regex.Matches(text, @"(?<!Days in )Cycle:\s+(.*?)(?=[A-Z][a-z]+, [A-Z][a-z]+|$)");

    foreach (Match match in matches)
      cycles.Add(match.Groups[1].Value.Trim());

    return cycles;
  }

  private List<string> ExtractRxNumbers(string text)
  {
    var rxNumbers = new List<string>();

    // Regex to find numbers followed by a month and remove last 2 digits before `-Month-`
    var matches = Regex.Matches(text, @"\b(\d+)(\d{2})(?=-\b(?:Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)-\d{4})");

    foreach (Match match in matches)
    {
      if (match.Success)
      {
        string rxNumber = match.Groups[1].Value;
        rxNumbers.Add(rxNumber);
      }
    }

    return rxNumbers;
  }


  private async Task<string> MoveDocument(string documentName, bool isSuccess = true)
  {
    using HttpClient client = new HttpClient();

    string authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_username}:{_password}"));
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);


    var destinationFolder = isSuccess ? "processing" : "failed";
    var sourceUrl = $"{_baseUrl}/pending/{documentName}";

    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(documentName);
    var extension = Path.GetExtension(documentName);


    var destinationUrl = $"{_baseUrl}/{destinationFolder}/{fileNameWithoutExtension}_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}{extension}";

    var request = new HttpRequestMessage(new HttpMethod("MOVE"), sourceUrl);
    request.Headers.Add("Destination", destinationUrl);

    var response = await client.SendAsync(request);
    if (!response.IsSuccessStatusCode)
      throw new Exception($"Failed to move {documentName} to {destinationFolder}. Status: {response.StatusCode}");

    return destinationUrl;
  }

  private async Task SaveDocument(string documentUrl, string documentName, Dictionary<string, string> metadata, DocumentStatus status, string? errorMessage = null)
  {
    var document = new Document
    {
      DocumentUrl = documentUrl,
      DocumentName = documentName,
      Status = status,
      CreatedDate = DateTime.UtcNow,
      IsDeleted = false,
      NoOfPatients = metadata.ContainsKey("UniqueNames") ? metadata["UniqueNames"].Split(", ").Length : 0
    };

    if (status == DocumentStatus.Failed && !string.IsNullOrEmpty(errorMessage))
    {
      document.Metadata.Add(new DocumentMetadata
      {
        Key = "ErrorMessage",
        Value = errorMessage,
        CreatedDate = DateTime.UtcNow,
      });
    }
    else
    {
      foreach (var meta in metadata)
      {
        document.Metadata.Add(new DocumentMetadata
        {
          Key = meta.Key,
          Value = meta.Value,
          CreatedDate = DateTime.UtcNow
        });
      }
    }

    using var scope = _serviceScopeFactory.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Document.Add(document);
    await dbContext.SaveChangesAsync();
  }

  public async Task<ApplicationResult<bool>> UploadSignedDocument(long documentId, IFormFile file)
  {
    if (file == null || file.Length == 0)
      return ApplicationResult<bool>.Error("No file uploaded.");

    // Fetch document from the database
    var document = await _documentRepository.Get()
        .FirstOrDefaultAsync(d => d.Id == documentId);

    if (document == null) return ApplicationResult<bool>.Error("No file uploaded.");

    // Ensure the document is in Processing status
    if (document.Status != DocumentStatus.PhysicalCheckRegTech && document.Status != DocumentStatus.DataEntryRegTech)
      return ApplicationResult<bool>.Error("Document is not in processing status.");

    // Extract file name from document URL
    string fileName = Path.GetFileName(document.DocumentUrl);
    string processingPath = $"{_baseUrl}/processing/{fileName}";

    // Delete the old document from the processing folder
    await DeleteDocumentFromProcessing(processingPath);

    var isProcessing = document.Status == DocumentStatus.DataEntryRegTech;

    // Upload new signed document to completed folder
    string newDocumentUrl = await UploadSignedDocumentToFolder(file, fileName, isProcessing);

    // Update document status to Completed
    document.DocumentUrl = newDocumentUrl;
    document.Status = isProcessing ? DocumentStatus.PhysicalCheckRegTech : DocumentStatus.Completed;
    document.ModifiedDate = DateTime.UtcNow;
    await _documentRepository.UpdateAsync(document);

    return ApplicationResult<bool>.SuccessResult(true);
  }

  public async Task<ApplicationResult<bool>> DeleteUnSignedDocument(long documentId)
  {
    // Fetch document from the database
    var document = await _documentRepository.Get()
        .FirstOrDefaultAsync(d => d.Id == documentId);

    if (document == null) return ApplicationResult<bool>.Error("No file uploaded.");

    // Ensure the document is in Processing status
    if (document.Status != DocumentStatus.DataEntryRegTech)
      return ApplicationResult<bool>.Error("Document can't be deleted.");

    // Extract file name from document URL
    string fileName = Path.GetFileName(document.DocumentUrl);
    string processingPath = $"{_baseUrl}/processing/{fileName}";

    // Delete the old document from the processing folder
    await DeleteDocumentFromProcessing(processingPath);
    await _documentRepository.DeleteAsync(document.Id);

    return ApplicationResult<bool>.SuccessResult(true);
  }


  private async Task DeleteDocumentFromProcessing(string processingPath)
  {
    using HttpClient client = new HttpClient();
    string authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_username}:{_password}"));
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
    var request = new HttpRequestMessage(HttpMethod.Delete, processingPath);
    var response = await client.SendAsync(request);
  }

  public async Task<ApplicationResult<bool>> UploadDocumentToCompleted(IFormFile file)
  {

    if (file == null || file.Length == 0)
      return ApplicationResult<bool>.Error("Unable to upload file");

    byte[] fileBytes;
    using (var memoryStream = new MemoryStream())
    {
      await file.CopyToAsync(memoryStream);
      fileBytes = memoryStream.ToArray();
    }

    var documentUrl = await UploadSignedDocumentToFolder(file, file.FileName);

    if (documentUrl == null) return ApplicationResult<bool>.Error("Unable to upload file");

    var metadata = ExtractDocumentMetadata(fileBytes);

    await SaveDocument(documentUrl, file.FileName, metadata, DocumentStatus.Completed);

    return ApplicationResult<bool>.SuccessResult(true);
  }

  private async Task<string> UploadSignedDocumentToFolder(IFormFile file, string fileName, bool isProcessing = false)
  {
    var folderName = isProcessing ? "processing" : "completed";
    string completedPath = $"{_baseUrl}/{folderName}/";
    using HttpClient client = new HttpClient();

    string authToken = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{_username}:{_password}"));
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

    using var fileStream = file.OpenReadStream();
    using var content = new StreamContent(fileStream);
    content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");


    HttpResponseMessage response = await client.PutAsync(completedPath + fileName, content);

    if (!response.IsSuccessStatusCode)
    {
      throw new Exception($"Failed to upload file. Status: {response.StatusCode} - {response.ReasonPhrase}");
    }

    return $"{completedPath}/{fileName}";
  }
}
