using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using Microsoft.Extensions.Configuration;
using PMS.API.Infrastructure.Data;

namespace PMS.API.Application.Common.Helpers;

public class EmailData
{
  public string? ToEmail { get; set; }
  public string? ToName { get; set; }
  public string? Name { get; set; }
  public string? Email { get; set; }
  public string? Password { get; set; }
  public string? link { get; set; }
  public string? ResetLink { get; set; }
}

public class EmailSenderService : IEmailSenderService
{
  public IConfiguration _configuration { get; set; }
  private AppDbContext _context { get; set; }
  public EmailSenderService(
    IConfiguration configuration,
    AppDbContext context)
  {
    _configuration = configuration;
    _context = context;
  }

  public async Task<bool> SendCredentialsEmail(EmailData model)
  {
    try
    {
      var smtpHost = _configuration["Smtp:Host"];
      var smtpPort = int.Parse(_configuration["Smtp:Port"] ?? "587");
      var smtpEmail = _configuration["Smtp:Email"];
      var smtpPassword = _configuration["Smtp:Password"];
      var senderName = _configuration["Smtp:SenderName"];
      if (smtpEmail == null) return false;

      using var smtpClient = new SmtpClient(smtpHost)
      {
        Port = smtpPort,
        EnableSsl = true,
        UseDefaultCredentials = false,
        Credentials = new NetworkCredential(smtpEmail, smtpPassword)
      };

      var mailMessage = new MailMessage
      {
        From = new MailAddress(smtpEmail, senderName),
        Subject = "Your Account Credentials",
        Body = GenerateEmailBody(model),
        IsBodyHtml = true
      };

      mailMessage.To.Add(new MailAddress(model.ToEmail ?? ""));

      await smtpClient.SendMailAsync(mailMessage);

      return true;
    }
    catch (Exception)
    {
      return false;
    }
  }

  public async Task<bool> SendResetPasswordEmail(EmailData model)
  {
    try
    {
      var smtpHost = _configuration["Smtp:Host"];
      var smtpPort = int.Parse(_configuration["Smtp:Port"] ?? "587");
      var smtpEmail = _configuration["Smtp:Email"];
      var smtpPassword = _configuration["Smtp:Password"];
      var senderName = _configuration["Smtp:SenderName"];

      using var smtpClient = new SmtpClient(smtpHost)
      {
        Port = smtpPort,
        Credentials = new NetworkCredential(smtpEmail, smtpPassword),
        EnableSsl = true
      };

      if (smtpEmail == null) return false;

      var mailMessage = new MailMessage
      {
        From = new MailAddress(smtpEmail, senderName),
        Subject = "Reset Your Password",
        Body = GenerateResetPasswordEmailBody(model),
        IsBodyHtml = true
      };

      mailMessage.To.Add(new MailAddress(model.ToEmail ?? ""));

      // Send the email
      await smtpClient.SendMailAsync(mailMessage);

      return true;
    }
    catch (Exception)
    {
      return false;
    }
  }

  private string GenerateResetPasswordEmailBody(EmailData model)
  {
    var sb = new StringBuilder();
    sb.AppendLine("<html><body>");
    sb.AppendLine($"<h2>Hello {model.ToName},</h2>");
    sb.AppendLine("<p>We received a request to reset your password. You can reset it by clicking the link below:</p>");
    sb.AppendLine($"<p><a href='{model.ResetLink}'>Reset Password</a></p>");
    sb.AppendLine("<p>If you didn’t request a password reset, please ignore this email or contact support if you have questions.</p>");
    sb.AppendLine("<p>Best regards,</p>");
    sb.AppendLine("<p>The PMS Team</p>");
    sb.AppendLine("</body></html>");
    return sb.ToString();
  }

  public async Task<bool> SendEmail(string toEmail, string subject, string body)
  {
    try
    {
      var smtpHost = _configuration["Smtp:Host"];
      var smtpPort = int.Parse(_configuration["Smtp:Port"] ?? "587");
      var smtpEmail = _configuration["Smtp:Email"];
      var smtpPassword = _configuration["Smtp:Password"];
      var senderName = _configuration["Smtp:SenderName"];
      if (smtpEmail == null) return false;

      using var smtpClient = new SmtpClient(smtpHost)
      {
        Port = smtpPort,
        EnableSsl = true,
        UseDefaultCredentials = false,
        Credentials = new NetworkCredential(smtpEmail, smtpPassword)
      };

      // Format the body as HTML, converting newlines to <br> tags
      var htmlBody = GenerateEmailHtmlBody(body);

      var mailMessage = new MailMessage
      {
        From = new MailAddress(smtpEmail, senderName),
        Subject = subject,
        Body = htmlBody,
        IsBodyHtml = true
      };

      mailMessage.To.Add(new MailAddress(toEmail));

      await smtpClient.SendMailAsync(mailMessage);

      return true;
    }
    catch (Exception)
    {
      return false;
    }
  }

  public async Task<bool> SendEmailWithAttachment(string toEmail, string toName, string subject, string body, string attachmentFilePath, string? attachmentFileName = null)
  {
    try
    {
      if (!File.Exists(attachmentFilePath))
        return false;

      var smtpHost = _configuration["Smtp:Host"];
      var smtpPort = int.Parse(_configuration["Smtp:Port"] ?? "587");
      var smtpEmail = _configuration["Smtp:Email"];
      var smtpPassword = _configuration["Smtp:Password"];
      var senderName = _configuration["Smtp:SenderName"];
      if (smtpEmail == null) return false;

      using var smtpClient = new SmtpClient(smtpHost)
      {
        Port = smtpPort,
        EnableSsl = true,
        UseDefaultCredentials = false,
        Credentials = new NetworkCredential(smtpEmail, smtpPassword)
      };

      var htmlBody = GenerateEmailHtmlBody(body);
      var mailMessage = new MailMessage
      {
        From = new MailAddress(smtpEmail, senderName),
        Subject = subject,
        Body = htmlBody,
        IsBodyHtml = true
      };
      mailMessage.To.Add(new MailAddress(toEmail));

      var attachment = new Attachment(attachmentFilePath, MediaTypeNames.Application.Octet);
      attachment.Name = attachmentFileName ?? Path.GetFileName(attachmentFilePath);
      mailMessage.Attachments.Add(attachment);

      await smtpClient.SendMailAsync(mailMessage);
      return true;
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex.ToString());
      return false;
    }
  }

  private string GenerateEmailHtmlBody(string message)
  {
    var sb = new StringBuilder();
    sb.AppendLine("<html><body>");
    sb.AppendLine("<div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>");
    // Convert newlines to <br> tags and escape HTML
    var escapedMessage = message
      .Replace("&", "&amp;")
      .Replace("<", "&lt;")
      .Replace(">", "&gt;")
      .Replace("\"", "&quot;")
      .Replace("'", "&#39;");
    var formattedMessage = escapedMessage.Replace("\r\n", "<br>").Replace("\n", "<br>").Replace("\r", "<br>");
    sb.AppendLine($"<p style='white-space: pre-wrap;'>{formattedMessage}</p>");
    sb.AppendLine("</div>");
    sb.AppendLine("</body></html>");
    return sb.ToString();
  }

  private string GenerateEmailBody(EmailData model)
  {
    var sb = new StringBuilder();
    sb.AppendLine("<html><body>");
    sb.AppendLine($"<h2>Hello {model.ToName},</h2>");
    sb.AppendLine("<p>We are pleased to share your account credentials with you.</p>");
    sb.AppendLine("<p>Here are your details:</p>");
    sb.AppendLine("<ul>");
    sb.AppendLine($"<li><strong>Name:</strong> {model.Name}</li>");
    sb.AppendLine($"<li><strong>Email:</strong> {model.Email}</li>");
    sb.AppendLine($"<li><strong>Password:</strong> <code>{model.Password}</code> </li>");
    sb.AppendLine("</ul>");
    sb.AppendLine($"<p>You can access your account using the following link: <a href='{model.link}'>{model.link}</a></p>");
    sb.AppendLine("<p>Best regards,</p>");
    sb.AppendLine("<p>The PMS Team</p>");
    sb.AppendLine("</body></html>");
    return sb.ToString();
  }
}
