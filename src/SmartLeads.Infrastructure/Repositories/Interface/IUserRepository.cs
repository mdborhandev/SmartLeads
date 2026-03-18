using SmartLeads.Domain.Models;

namespace SmartLeads.Infrastructure.Repositories.Interface;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByUsernameAndCompanyIdAsync(string username, Guid companyId, CancellationToken token = default);
    Task<User?> GetUserByIdAndCompanyIdAsync(Guid id, Guid companyId, CancellationToken token = default);
    Task<IEnumerable<User>> GetAllUsersAsync();
}
