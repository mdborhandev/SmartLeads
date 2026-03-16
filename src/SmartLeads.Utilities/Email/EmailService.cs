using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using SmartLeads.Utilities.Interfaces;

namespace SmartLeads.Utilities.Email;

public class EmailService : IEmailService
{
    private readonly SMTPConfigModel _smtpConfig;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<SMTPConfigModel> smtpConfigOptions, ILogger<EmailService> logger)
    {
        _smtpConfig = smtpConfigOptions.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtpConfig.SenderName, _smtpConfig.SenderEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlMessage
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            var secureSocketOptions = _smtpConfig.SmtpPort == 465
                ? SecureSocketOptions.SslOnConnect
                : _smtpConfig.SmtpPort == 587
                    ? SecureSocketOptions.StartTls
                    : SecureSocketOptions.None;

            await client.ConnectAsync(_smtpConfig.SmtpServer, _smtpConfig.SmtpPort, secureSocketOptions);

            if (!string.IsNullOrEmpty(_smtpConfig.SmtpUsername))
            {
                await client.AuthenticateAsync(_smtpConfig.SmtpUsername, _smtpConfig.SmtpPassword);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}. Error: {Error}", toEmail, ex.Message);
            throw;
        }
    }

    public async Task SendEmailAsync(List<string> toEmails, string subject, string htmlMessage)
    {
        if (toEmails == null || toEmails.Count == 0)
        {
            _logger.LogWarning("Attempted to send email to empty recipient list");
            return;
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtpConfig.SenderName, _smtpConfig.SenderEmail));
            message.Subject = subject;

            foreach (var email in toEmails)
            {
                message.To.Add(new MailboxAddress("", email));
            }

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlMessage
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            var secureSocketOptions = _smtpConfig.SmtpPort == 465
                ? SecureSocketOptions.SslOnConnect
                : _smtpConfig.SmtpPort == 587
                    ? SecureSocketOptions.StartTls
                    : SecureSocketOptions.None;

            await client.ConnectAsync(_smtpConfig.SmtpServer, _smtpConfig.SmtpPort, secureSocketOptions);

            if (!string.IsNullOrEmpty(_smtpConfig.SmtpUsername))
            {
                await client.AuthenticateAsync(_smtpConfig.SmtpUsername, _smtpConfig.SmtpPassword);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent successfully to {Count} recipients", toEmails.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Count} recipients. Error: {Error}", toEmails.Count, ex.Message);
            throw;
        }
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage, Stream fileStream, string fileName)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtpConfig.SenderName, _smtpConfig.SenderEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlMessage
            };

            if (fileStream != null && fileStream.Length > 0)
            {
                fileStream.Position = 0;
                var attachment = new MimePart()
                {
                    Content = new MimeContent(fileStream, ContentEncoding.Default),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                    FileName = fileName
                };
                bodyBuilder.Attachments.Add(attachment);
            }

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            var secureSocketOptions = _smtpConfig.SmtpPort == 465
                ? SecureSocketOptions.SslOnConnect
                : _smtpConfig.SmtpPort == 587
                    ? SecureSocketOptions.StartTls
                    : SecureSocketOptions.None;

            await client.ConnectAsync(_smtpConfig.SmtpServer, _smtpConfig.SmtpPort, secureSocketOptions);

            if (!string.IsNullOrEmpty(_smtpConfig.SmtpUsername))
            {
                await client.AuthenticateAsync(_smtpConfig.SmtpUsername, _smtpConfig.SmtpPassword);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email with attachment sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email with attachment to {Email}. Error: {Error}", toEmail, ex.Message);
            throw;
        }
    }
}
