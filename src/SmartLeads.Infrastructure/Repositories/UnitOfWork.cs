using SmartLeads.Infrastructure.Persistence;
using SmartLeads.Infrastructure.Repositories.Implementation;
using SmartLeads.Infrastructure.Repositories.Interface;

namespace SmartLeads.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    protected readonly SystemDbContext _systemDbContext;
    protected readonly CompanyDbContext _companyDbContext;

    #region Repositories
    public IContactRepository contactRepository { get; private set; }
    public IUserRepository userRepository { get; private set; }
    public ICompanyRepository companyRepository { get; private set; }
    public IInvitationRepository invitationRepository { get; private set; }
    public IColumnFilterRepository columnFilterRepository { get; private set; }
    public SystemDbContext systemDbContext => _systemDbContext;
    #endregion

    public UnitOfWork(SystemDbContext systemDbContext, CompanyDbContext companyDbContext)
    {
        _systemDbContext = systemDbContext;
        _companyDbContext = companyDbContext;
        
        // System repositories
        userRepository = new UserRepository(systemDbContext);
        companyRepository = new CompanyRepository(systemDbContext);
        invitationRepository = new InvitationRepository(systemDbContext);
        
        // Company repositories
        contactRepository = new ContactRepository(companyDbContext);
        columnFilterRepository = new ColumnFilterRepository(companyDbContext);
    }

    public async Task SaveAsync(CancellationToken token = default)
    {
        // Save both contexts (in real scenario, consider distributed transactions)
        await _systemDbContext.SaveChangesAsync(token);
        await _companyDbContext.SaveChangesAsync(token);
    }

    public async ValueTask DisposeAsync()
    {
        await _systemDbContext.DisposeAsync();
        await _companyDbContext.DisposeAsync();
    }
}
