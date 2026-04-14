using SmartLeads.Domain.Models;

namespace SmartLeads.Utilities.Interfaces;

/// <summary>
/// Minimal user repository interface for use by INotificationService.
/// Full implementation lives in Infrastructure.
/// </summary>
public interface IUserRepo
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken token = default);
}
