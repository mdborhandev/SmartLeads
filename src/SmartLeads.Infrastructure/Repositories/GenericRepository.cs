using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Persistence;
using SmartLeads.Infrastructure.Repositories.Interface;

namespace SmartLeads.Infrastructure.Repositories;

// Generic repository for system entities (User, Company, etc.)
public class GenericSystemRepository<TEntity> : BaseRepository<TEntity, Guid>, IGenericSystemRepository<TEntity>
    where TEntity : BaseSystemEntity
{
    public GenericSystemRepository(SystemDbContext dbContext) : base(dbContext)
    {
    }
}

// Generic repository for default database entities (Contact, Group, Tag, etc.)
public class GenericCompanyRepository<TEntity> : BaseRepository<TEntity, Guid>, IGenericCompanyRepository<TEntity>
    where TEntity : BaseEntity
{
    protected readonly DefaultDbContext _defaultDbContext;

    public GenericCompanyRepository(DefaultDbContext dbContext) : base(dbContext)
    {
        _defaultDbContext = dbContext;
    }
}

// Backward compatible alias
public class GenericRepository<TEntity> : GenericCompanyRepository<TEntity> where TEntity : BaseEntity
{
    public GenericRepository(DefaultDbContext dbContext) : base(dbContext)
    {
    }
}
