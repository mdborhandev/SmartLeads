namespace SmartLeads.Utilities.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string username, string resetLink);
}
