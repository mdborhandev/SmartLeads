namespace SmartLeads.Domain.Interfaces.Services;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string username, string resetLink);
}
