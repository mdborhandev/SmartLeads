namespace SmartLeads.Domain.Interfaces.Repositories;

public interface IUnitOfWork : IDisposable
{
    // Individual repositories will be added here as we create entities
    // e.g., IContactRepository Contacts { get; }
    
    Task<int> CompleteAsync();
}
