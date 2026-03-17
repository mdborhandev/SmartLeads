using SmartLeads.Domain.Models;

namespace SmartLeads.Infrastructure.Repositories.Interface;

public interface IInvitationRepository : IBaseRepository<Invitation, Guid>
{
    Task<Invitation?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<Invitation?> GetByEmailAndTokenAsync(string email, string token, CancellationToken cancellationToken = default);
    Task<IList<Invitation>> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<IList<Invitation>> GetPendingByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<IList<Invitation>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
