using Microsoft.EntityFrameworkCore;
using SmartLeads.Domain.DTOs;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Persistence;
using SmartLeads.Infrastructure.Repositories.Interface;
using System.Text.Json;

namespace SmartLeads.Infrastructure.Repositories.Implementation;

public class InvitationRepository : BaseRepository<Invitation, Guid>, IInvitationRepository
{
    private readonly DefaultDbContext _defaultDbContext;
    private readonly SystemDbContext _systemDbContext;

    public InvitationRepository(DefaultDbContext dbContext, SystemDbContext systemDbContext) : base(dbContext)
    {
        _defaultDbContext = dbContext;
        _systemDbContext = systemDbContext;
    }

    public async Task<Invitation?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await SingleOrDefaultAsync(i => i.Token == token && !i.IsDeleted, cancellationToken);
    }

    public async Task<Invitation?> GetByEmailAndTokenAsync(string email, string token, CancellationToken cancellationToken = default)
    {
        return await SingleOrDefaultAsync(i => i.Email == email && i.Token == token && !i.IsDeleted, cancellationToken);
    }

    public async Task<IList<Invitation>> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await GetByConditionAsync(i => i.CompanyId == companyId && !i.IsDeleted, cancellationToken);
    }

    public async Task<IList<Invitation>> GetPendingByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await GetByConditionAsync(
            i => i.CompanyId == companyId && !i.IsDeleted && i.Status == InvitationStatus.Pending,
            cancellationToken
        );
    }

    public async Task<IList<Invitation>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await GetByConditionAsync(i => i.InvitedByUserId == userId && !i.IsDeleted, cancellationToken);
    }

    // Business logic methods
    public async Task<Invitation?> GetPendingInvitationByEmailAndCompanyIdAsync(string email, Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _systemDbContext.Invitations
            .FirstOrDefaultAsync(i => i.Email.ToLower() == email.ToLower() 
                && i.CompanyId == companyId 
                && i.Status == InvitationStatus.Pending
                && !i.IsDeleted, cancellationToken);
    }

    public async Task<bool> MarkInvitationAsAcceptedAsync(Guid invitationId, CancellationToken cancellationToken = default)
    {
        var invitation = await GetByIdAsync(invitationId, cancellationToken);
        if (invitation == null)
        {
            return false;
        }

        invitation.IsAccepted = true;
        invitation.AcceptedAt = DateTime.UtcNow;
        invitation.Status = InvitationStatus.Accepted;
        await EditAsync(invitation);
        return true;
    }

    public async Task<bool> MarkInvitationAsRejectedAsync(Guid invitationId, string? reason, CancellationToken cancellationToken = default)
    {
        var invitation = await GetByIdAsync(invitationId, cancellationToken);
        if (invitation == null)
        {
            return false;
        }

        invitation.Status = InvitationStatus.Rejected;
        invitation.RejectedReason = reason;
        await EditAsync(invitation);
        return true;
    }

    public async Task<bool> MarkInvitationAsCancelledAsync(Guid invitationId, CancellationToken cancellationToken = default)
    {
        var invitation = await GetByIdAsync(invitationId, cancellationToken);
        if (invitation == null)
        {
            return false;
        }

        invitation.Status = InvitationStatus.Cancelled;
        await EditAsync(invitation);
        return true;
    }

    public async Task<IList<InvitationDto>> GetInvitationsDtoByCompanyIdAsync(Guid companyId, bool pendingOnly, CancellationToken cancellationToken = default)
    {
        var invitations = pendingOnly
            ? await GetPendingByCompanyIdAsync(companyId, cancellationToken)
            : await GetByCompanyIdAsync(companyId, cancellationToken);

        var dtos = new List<InvitationDto>();
        foreach (var invitation in invitations)
        {
            var invitedByUser = await _systemDbContext.Users.FindAsync(new object[] { invitation.InvitedByUserId }, cancellationToken);
            var invitedByUserName = invitedByUser != null
                ? (!string.IsNullOrEmpty(invitedByUser.FirstName) && !string.IsNullOrEmpty(invitedByUser.LastName)
                    ? $"{invitedByUser.FirstName} {invitedByUser.LastName}"
                    : invitedByUser.Username)
                : "Unknown";

            dtos.Add(new InvitationDto
            {
                Id = invitation.Id,
                Email = invitation.Email,
                Role = invitation.Role,
                InvitedByUserName = invitedByUserName,
                InvitedAt = invitation.CreatedAt,
                ExpiresAt = invitation.ExpiresAt,
                IsAccepted = invitation.IsAccepted,
                AcceptedAt = invitation.AcceptedAt,
                Status = invitation.Status.ToString()
            });
        }

        return dtos;
    }
}
