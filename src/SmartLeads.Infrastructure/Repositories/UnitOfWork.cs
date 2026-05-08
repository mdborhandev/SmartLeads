using SmartLeads.Infrastructure.Persistence;
using SmartLeads.Infrastructure.Repositories.Implementation;
using SmartLeads.Infrastructure.Repositories.Interface;
using IPasswordHasher = SmartLeads.Utilities.Interfaces.IPasswordHasher;

namespace SmartLeads.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    protected readonly SystemDbContext _systemDbContext;
    protected readonly DefaultDbContext _defaultDbContext;
    private readonly IPasswordHasher _passwordHasher;

    #region Repositories
    public IUserRepository userRepository { get; private set; }
    public ICompanyRepository companyRepository { get; private set; }
    public IInvitationRepository invitationRepository { get; private set; }
    public IColumnFilterRepository columnFilterRepository { get; private set; }
    public IDepartmentRepository departmentRepository { get; private set; }
    public IDesignationRepository designationRepository { get; private set; }
    public INotificationRepository notificationRepository { get; private set; }
    public INotificationPreferenceRepository notificationPreferenceRepository { get; private set; }
    public IEmployeeRepository employeeRepository { get; private set; }
    public IVariableRepository variableRepository { get; private set; }
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
        notificationRepository = new NotificationRepository(systemDbContext);
        notificationPreferenceRepository = new NotificationPreferenceRepository(systemDbContext);

        // Default database repositories
        columnFilterRepository = new ColumnFilterRepository(defaultDbContext);
        departmentRepository = new DepartmentRepository(defaultDbContext);
        designationRepository = new DesignationRepository(defaultDbContext);
        employeeRepository = new EmployeeRepository(defaultDbContext);
        variableRepository = new VariableRepository(defaultDbContext);
    }

    public async Task SaveAsync(CancellationToken token = default)
    {
        // Save both contexts - system context first, then default context
        await _systemDbContext.SaveChangesAsync(token);
        await _defaultDbContext.SaveChangesAsync(token);
    }

    public async ValueTask DisposeAsync()
    {
        await _systemDbContext.DisposeAsync();
        await _defaultDbContext.DisposeAsync();
    }
}
