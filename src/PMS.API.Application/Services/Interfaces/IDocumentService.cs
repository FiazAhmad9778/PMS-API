
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMS.API.Application.Common.Models;

namespace PMS.API.Application.Services.Interfaces;
public interface IDocumentService
{
  Task<ApplicationResult<bool>> DeleteUnSignedDocument(long documentId);
  Task<FileContentResult> GetDocumentPdfAsync(long documentId);
  Task SyncDocuments(CancellationToken stoppingToken);
  Task<ApplicationResult<bool>> UploadDocumentToCompleted(IFormFile file);
  Task<ApplicationResult<bool>> UploadSignedDocument(long documentId, IFormFile file);
}
