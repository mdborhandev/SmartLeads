using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Persistence;
using SmartLeads.Infrastructure.Repositories.Interface;

namespace SmartLeads.Infrastructure.Repositories;

public class GenericRepository<TEntity> : BaseRepository<TEntity, Guid>, IGenericRepository<TEntity> where TEntity : BaseEntity
{
    public GenericRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}
