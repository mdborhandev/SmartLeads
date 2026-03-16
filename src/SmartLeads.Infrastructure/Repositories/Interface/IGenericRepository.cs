using SmartLeads.Domain.Models;

namespace SmartLeads.Infrastructure.Repositories.Interface;

public interface IGenericRepository<TEntity> : IBaseRepository<TEntity, Guid> where TEntity : BaseEntity
{
}
