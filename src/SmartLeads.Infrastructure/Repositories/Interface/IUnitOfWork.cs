using SmartLeads.Infrastructure.Persistence;

namespace SmartLeads.Infrastructure.Repositories.Interface;

public interface IUnitOfWork : IAsyncDisposable
{
    IContactRepository contactRepository { get; }
    IUserRepository userRepository { get; }
    ICompanyRepository companyRepository { get; }
    IInvitationRepository invitationRepository { get; }
    IColumnFilterRepository columnFilterRepository { get; }
    IDepartmentRepository departmentRepository { get; }
    IDesignationRepository designationRepository { get; }
    INotificationRepository notificationRepository { get; }
    INotificationPreferenceRepository notificationPreferenceRepository { get; }

    SystemDbContext systemDbContext { get; }
    DefaultDbContext defaultDbContext { get; }

    Task SaveAsync(CancellationToken token = default);
}
