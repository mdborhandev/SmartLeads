namespace SmartLeads.Utilities.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
    Task SendEmailAsync(List<string> toEmails, string subject, string htmlMessage);
    Task SendEmailAsync(string toEmail, string subject, string htmlMessage, Stream fileStream, string fileName);
}
