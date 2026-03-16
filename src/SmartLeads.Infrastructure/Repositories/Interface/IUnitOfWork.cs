namespace SmartLeads.Infrastructure.Repositories.Interface;

public interface IUnitOfWork : IAsyncDisposable
{
    IContactRepository contactRepository { get; }

    Task SaveAsync(CancellationToken token = default);
}
