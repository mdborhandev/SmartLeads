using Microsoft.EntityFrameworkCore;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Persistence;
using SmartLeads.Infrastructure.Repositories.Interface;

namespace SmartLeads.Infrastructure.Repositories.Implementation;

public class InvitationRepository : BaseRepository<Invitation, Guid>, IInvitationRepository
{
    public InvitationRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
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
}
