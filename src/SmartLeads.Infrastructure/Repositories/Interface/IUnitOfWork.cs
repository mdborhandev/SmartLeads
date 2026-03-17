namespace SmartLeads.Infrastructure.Repositories.Interface;

public interface IUnitOfWork : IAsyncDisposable
{
    IContactRepository contactRepository { get; }
    IUserRepository userRepository { get; }
    ICompanyRepository companyRepository { get; }
    IInvitationRepository invitationRepository { get; }

    Task SaveAsync(CancellationToken token = default);
}
