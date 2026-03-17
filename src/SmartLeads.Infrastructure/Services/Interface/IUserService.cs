namespace SmartLeads.Infrastructure.Services.Interface;

public interface IUserService
{
    Task<bool> SendPasswordResetEmailAsync(string email, string subject, string emailBody, string baseUrl);
    Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
    Task<(bool Success, string? Token, string? Error)> LoginAsync(string usernameOrEmail, string password);
    Task<(bool Success, string? Token, string? Error)> RegisterAsync(string username, string email, string password, string firstName, string lastName, Guid? companyId = null);
    Task<Domain.Models.User?> GetUserByUsernameOrEmailAsync(string usernameOrEmail);
    Task<bool> UpdateProfileAsync(string username, string email, string firstName, string lastName);
}
