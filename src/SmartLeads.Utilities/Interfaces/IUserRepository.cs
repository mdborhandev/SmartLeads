using SmartLeads.Domain.Models;

namespace SmartLeads.Utilities.Interfaces;

/// <summary>
/// User repository interface for use by INotificationService.
/// Full implementation lives in Infrastructure.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken token = default);
}
