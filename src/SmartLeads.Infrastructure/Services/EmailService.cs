using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartLeads.Application.Common.Interfaces;

namespace SmartLeads.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string username, string resetLink)
    {
        var emailSettings = _configuration.GetSection("EmailSettings");
        var smtpServer = emailSettings["SmtpServer"];
        var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
        var senderEmail = emailSettings["SenderEmail"];
        var senderName = emailSettings["SenderName"] ?? "SmartLeads";
        var smtpUsername = emailSettings["SmtpUsername"];
        var smtpPassword = emailSettings["SmtpPassword"];
        var enableSsl = bool.Parse(emailSettings["EnableSsl"] ?? "true");

        var subject = "Password Reset Request - SmartLeads";
        var body = $@"
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 8px 8px; }}
        .button {{ display: inline-block; background: #667eea; color: white; padding: 14px 32px; text-decoration: none; border-radius: 6px; margin: 20px 0; font-weight: bold; }}
        .button:hover {{ background: #5a6fd6; }}
        .footer {{ text-align: center; margin-top: 20px; color: #888; font-size: 12px; }}
        .warning {{ background: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; border-radius: 4px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔐 Password Reset Request</h1>
        </div>
        <div class='content'>
            <p>Hello {username},</p>
            <p>We received a request to reset your SmartLeads account password. Click the button below to reset your password:</p>
            
            <div style='text-align: center;'>
                <a href='{resetLink}' class='button'>Reset Password</a>
            </div>
            
            <p>Or copy and paste this link into your browser:</p>
            <p style='word-break: break-all; color: #667eea;'>{resetLink}</p>
            
            <div class='warning'>
                <strong>⚠️ Important:</strong> This link will expire in 24 hours. If you didn't request this password reset, you can safely ignore this email. Your password will remain unchanged.
            </div>
            
            <p>Best regards,<br><strong>The SmartLeads Team</strong></p>
        </div>
        <div class='footer'>
            <p>&copy; {DateTime.Now.Year} SmartLeads. All rights reserved.</p>
            <p>This is an automated message, please do not reply.</p>
        </div>
    </div>
</body>
</html>";

        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(senderEmail!, senderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(new MailAddress(toEmail));

            using var client = new SmtpClient(smtpServer!, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                EnableSsl = enableSsl
            };

            await client.SendMailAsync(message);
            _logger.LogInformation("Password reset email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}. Error: {Error}", toEmail, ex.Message);
            throw;
        }
    }
}
