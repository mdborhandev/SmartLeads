using SmartLeads.Infrastructure.Persistence;

namespace SmartLeads.Infrastructure.Repositories.Interface;

public interface IUnitOfWork : IAsyncDisposable
{
    IContactRepository contactRepository { get; }
    IUserRepository userRepository { get; }
    ICompanyRepository companyRepository { get; }
    IInvitationRepository invitationRepository { get; }
    IColumnFilterRepository columnFilterRepository { get; }
    
    SystemDbContext systemDbContext { get; }

    Task SaveAsync(CancellationToken token = default);
}
