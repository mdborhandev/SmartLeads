using SmartLeads.Domain.DTOs;
using SmartLeads.Domain.Models;

namespace SmartLeads.Infrastructure.Repositories.Interface;

public interface IInvitationRepository : IBaseRepository<Invitation, Guid>
{
    Task<Invitation?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<Invitation?> GetByEmailAndTokenAsync(string email, string token, CancellationToken cancellationToken = default);
    Task<IList<Invitation>> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<IList<Invitation>> GetPendingByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<IList<Invitation>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    
    // Business logic methods
    Task<Invitation?> GetPendingInvitationByEmailAndCompanyIdAsync(string email, Guid companyId, CancellationToken cancellationToken = default);
    Task<bool> MarkInvitationAsAcceptedAsync(Guid invitationId, CancellationToken cancellationToken = default);
    Task<bool> MarkInvitationAsRejectedAsync(Guid invitationId, string? reason, CancellationToken cancellationToken = default);
    Task<bool> MarkInvitationAsCancelledAsync(Guid invitationId, CancellationToken cancellationToken = default);
    Task<IList<InvitationDto>> GetInvitationsDtoByCompanyIdAsync(Guid companyId, bool pendingOnly, CancellationToken cancellationToken = default);
}
