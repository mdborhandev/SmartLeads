using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Persistence;
using SmartLeads.Infrastructure.Repositories.Interface;

namespace SmartLeads.Infrastructure.Repositories;

public class GenericSystemRepository<TEntity> : BaseRepository<TEntity, Guid>, IGenericSystemRepository<TEntity>
    where TEntity : BaseSystemEntity
{
    public GenericSystemRepository(SmartLeadsDbContext dbContext) : base(dbContext)
    {
    }
}

public class GenericCompanyRepository<TEntity> : BaseRepository<TEntity, Guid>, IGenericCompanyRepository<TEntity>
    where TEntity : BaseEntity
{
    public GenericCompanyRepository(SmartLeadsDbContext dbContext) : base(dbContext)
    {
    }
}

public class GenericRepository<TEntity> : GenericCompanyRepository<TEntity> where TEntity : BaseEntity
{
    public GenericRepository(SmartLeadsDbContext dbContext) : base(dbContext)
    {
    }
}
