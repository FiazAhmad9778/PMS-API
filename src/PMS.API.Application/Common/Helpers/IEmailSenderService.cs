namespace PMS.API.Application.Common.Helpers;

public interface IEmailSenderService
{
  Task<bool> SendCredentialsEmail(EmailData model);
  Task<bool> SendResetPasswordEmail(EmailData model);
}
