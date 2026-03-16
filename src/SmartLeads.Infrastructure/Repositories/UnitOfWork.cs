using SmartLeads.Infrastructure.Persistence;
using SmartLeads.Infrastructure.Repositories.Implementation;
using SmartLeads.Infrastructure.Repositories.Interface;

namespace SmartLeads.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    protected readonly ApplicationDbContext _dbContext;

    #region Repositories
    public IContactRepository contactRepository { get; private set; }
    #endregion

    public UnitOfWork(ApplicationDbContext context)
    {
        _dbContext = context;
        contactRepository = new ContactRepository(context);
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
