using SmartLeads.Domain.DTOs;
using SmartLeads.Domain.Models;

namespace SmartLeads.Infrastructure.Repositories.Interface;

public interface IUserRepository : IGenericSystemRepository<User>, SmartLeads.Utilities.Interfaces.IUserRepository
{
    new Task<User?> GetByIdAsync(Guid id, CancellationToken token = default);
    Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByUsernameAndCompanyIdAsync(string username, Guid companyId, CancellationToken token = default);
    Task<User?> GetUserByIdAndCompanyIdAsync(Guid id, Guid companyId, CancellationToken token = default);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<PaginationResponse<UserTableDto>> GetUsersPagedAsync(Guid companyId, PaginationRequest request, CancellationToken token = default);

    // Authentication and profile methods
    Task<bool> VerifyPasswordAsync(string password, string passwordHash);
    Task<bool> UpdateProfileAsync(User user, CancellationToken token = default);
    Task<IEnumerable<UserCompany>> GetUserCompaniesAsync(Guid userId, CancellationToken token = default);

    // Password reset methods
    Task<bool> SetPasswordResetTokenAsync(string email, string token, DateTime expiryTime, CancellationToken cancellationToken = default);
    Task<bool> ResetPasswordAsync(string email, string token, string newPasswordHash, CancellationToken cancellationToken = default);

    // Uniqueness checks (global, not company-specific)
    Task<bool> IsUsernameTakenAsync(string username, Guid? excludeUserId = null, CancellationToken token = default);
    Task<bool> IsEmailTakenAsync(string email, Guid? excludeUserId = null, CancellationToken token = default);

    // Password change
    Task<bool> ChangePasswordAsync(Guid userId, string currentPasswordHash, string newPasswordHash, CancellationToken token = default);
}
