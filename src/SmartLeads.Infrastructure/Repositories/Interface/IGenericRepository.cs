using SmartLeads.Domain.Models;

namespace SmartLeads.Infrastructure.Repositories.Interface;

// Generic repository for system entities (User, Company, etc.)
public interface IGenericSystemRepository<TEntity> : IBaseRepository<TEntity, Guid> where TEntity : BaseSystemEntity
{
}

// Generic repository for company entities (Contact, Group, Tag, etc.)
public interface IGenericCompanyRepository<TEntity> : IBaseRepository<TEntity, Guid> where TEntity : BaseEntity
{
}

// Backward compatible alias
public interface IGenericRepository<TEntity> : IGenericCompanyRepository<TEntity> where TEntity : BaseEntity
{
}
