using PMS.API.Core.Domain.Entities;

namespace PMS.API.Application.Services.Interfaces;

public interface IPdfService
{
  Task<byte[]> GenerateOrderPdfAsync(Order order);

  Task<byte[]> GenerateContactFormPdfAsync(
    string contactName,
    string contactLastName,
    string contactPhone,
    string contactEmail,
    string? notes);

  Task<byte[]> GenerateTransferRequestPdfAsync(
    string firstName,
    string lastName,
    string phoneNumber,
    string? phoneNumber2,
    string? dateOfBirth,
    string? transferringFromPharmacy,
    string? notesOrConcerns);
}


