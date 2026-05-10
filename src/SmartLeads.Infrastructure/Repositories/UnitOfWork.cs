using SmartLeads.Infrastructure.Persistence;
using SmartLeads.Infrastructure.Repositories.Implementation;
using SmartLeads.Infrastructure.Repositories.Interface;
using IPasswordHasher = SmartLeads.Utilities.Interfaces.IPasswordHasher;

namespace SmartLeads.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    protected readonly SmartLeadsDbContext _dbContext;
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
    public SmartLeadsDbContext dbContext => _dbContext;
    #endregion

    public UnitOfWork(SmartLeadsDbContext dbContext, IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;

        userRepository = new UserRepository(dbContext, passwordHasher);
        companyRepository = new CompanyRepository(dbContext);
        invitationRepository = new InvitationRepository(dbContext);
        notificationRepository = new NotificationRepository(dbContext);
        notificationPreferenceRepository = new NotificationPreferenceRepository(dbContext);

        columnFilterRepository = new ColumnFilterRepository(dbContext);
        departmentRepository = new DepartmentRepository(dbContext);
        designationRepository = new DesignationRepository(dbContext);
        employeeRepository = new EmployeeRepository(dbContext);
        variableRepository = new VariableRepository(dbContext);
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
