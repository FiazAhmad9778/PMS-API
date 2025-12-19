namespace PMS.API.Application.Services.Interfaces;

public interface IInterFaxService
{
  Task<(bool success, string? transactionId, string? errorMessage)> SendFaxAsync(
    string faxNumber,
    byte[] pdfBytes,
    string? fileName = null);
  
  Task<(bool success, string? status, string? errorMessage)> GetFaxStatusAsync(string transactionId);
}









