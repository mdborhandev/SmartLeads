using SmartLeads.Infrastructure.Persistence;
using SmartLeads.Infrastructure.Repositories.Implementation;
using SmartLeads.Infrastructure.Repositories.Interface;

namespace SmartLeads.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    protected readonly ApplicationDbContext _dbContext;

    #region Repositories
    public IContactRepository contactRepository { get; private set; }
    public IUserRepository userRepository { get; private set; }
    public ICompanyRepository companyRepository { get; private set; }
    public IInvitationRepository invitationRepository { get; private set; }
    #endregion

    public UnitOfWork(ApplicationDbContext context)
    {
        _dbContext = context;
        contactRepository = new ContactRepository(context);
        userRepository = new UserRepository(context);
        companyRepository = new CompanyRepository(context);
        invitationRepository = new InvitationRepository(context);
    }

    public async Task SaveAsync(CancellationToken token = default)
    {
        await _dbContext.SaveChangesAsync(token);
    }

    public async ValueTask DisposeAsync()
    {
        await _dbContext.DisposeAsync();
    }
}
