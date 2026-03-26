using SmartLeads.Infrastructure.Persistence;
using SmartLeads.Infrastructure.Repositories.Implementation;
using SmartLeads.Infrastructure.Repositories.Interface;
using SmartLeads.Utilities.Interfaces;

namespace SmartLeads.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    protected readonly SystemDbContext _systemDbContext;
    protected readonly DefaultDbContext _defaultDbContext;
    private readonly IPasswordHasher _passwordHasher;

    #region Repositories
    public IContactRepository contactRepository { get; private set; }
    public IUserRepository userRepository { get; private set; }
    public ICompanyRepository companyRepository { get; private set; }
    public IInvitationRepository invitationRepository { get; private set; }
    public IColumnFilterRepository columnFilterRepository { get; private set; }
    public SystemDbContext systemDbContext => _systemDbContext;
    public DefaultDbContext defaultDbContext => _defaultDbContext;
    #endregion

    public UnitOfWork(SystemDbContext systemDbContext, DefaultDbContext defaultDbContext, IPasswordHasher passwordHasher)
    {
        _systemDbContext = systemDbContext;
        _defaultDbContext = defaultDbContext;
        _passwordHasher = passwordHasher;

        // System repositories
        userRepository = new UserRepository(systemDbContext, defaultDbContext, passwordHasher);
        companyRepository = new CompanyRepository(systemDbContext);
        invitationRepository = new InvitationRepository(defaultDbContext, systemDbContext);

        // Default database repositories
        contactRepository = new ContactRepository(defaultDbContext);
        columnFilterRepository = new ColumnFilterRepository(defaultDbContext);
    }

    public async Task SaveAsync(CancellationToken token = default)
    {
        // Save system context first
        var systemChanges = await _systemDbContext.SaveChangesAsync(token);
        
        // If system save succeeded, save default context
        if (systemChanges > 0)
        {
            await _defaultDbContext.SaveChangesAsync(token);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _systemDbContext.DisposeAsync();
        await _defaultDbContext.DisposeAsync();
    }
}
