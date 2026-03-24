using SmartLeads.Infrastructure.Persistence;
using SmartLeads.Infrastructure.Repositories.Implementation;
using SmartLeads.Infrastructure.Repositories.Interface;

namespace SmartLeads.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    protected readonly SystemDbContext _systemDbContext;
    protected readonly DefaultDbContext _defaultDbContext;

    #region Repositories
    public IContactRepository contactRepository { get; private set; }
    public IUserRepository userRepository { get; private set; }
    public ICompanyRepository companyRepository { get; private set; }
    public IInvitationRepository invitationRepository { get; private set; }
    public IColumnFilterRepository columnFilterRepository { get; private set; }
    public SystemDbContext systemDbContext => _systemDbContext;
    public DefaultDbContext defaultDbContext => _defaultDbContext;
    #endregion

    public UnitOfWork(SystemDbContext systemDbContext, DefaultDbContext defaultDbContext)
    {
        _systemDbContext = systemDbContext;
        _defaultDbContext = defaultDbContext;
        
        // System repositories
        userRepository = new UserRepository(systemDbContext, defaultDbContext);
        companyRepository = new CompanyRepository(systemDbContext);
        invitationRepository = new InvitationRepository(defaultDbContext);
        
        // Default database repositories
        contactRepository = new ContactRepository(defaultDbContext);
        columnFilterRepository = new ColumnFilterRepository(defaultDbContext);
    }

    public async Task SaveAsync(CancellationToken token = default)
    {
        // Save both contexts (in real scenario, consider distributed transactions)
        await _systemDbContext.SaveChangesAsync(token);
        await _defaultDbContext.SaveChangesAsync(token);
    }

    public async ValueTask DisposeAsync()
    {
        await _systemDbContext.DisposeAsync();
        await _defaultDbContext.DisposeAsync();
    }
}
