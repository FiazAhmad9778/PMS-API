namespace PMS.API.Application.Common.Helpers;

public interface IEmailSenderService
{
  Task<bool> SendCredentialsEmail(EmailData model);
  Task<bool> SendResetPasswordEmail(EmailData model);
  Task<bool> SendEmail(string toEmail, string subject, string body);
  /// <summary>Sends an email with a file attachment (e.g. invoice). Uses same SMTP as password reset.</summary>
  Task<bool> SendEmailWithAttachment(string toEmail, string toName, string subject, string body, string attachmentFilePath, string? attachmentFileName = null);
}
