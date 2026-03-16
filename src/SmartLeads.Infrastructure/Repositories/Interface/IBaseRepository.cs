using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace SmartLeads.Infrastructure.Repositories.Interface;

public interface IBaseRepository<TEntity, TKey> where TEntity : class
{
    // Add operations
    void Add(TEntity entity);
    Task AddAsync(TEntity entity, CancellationToken token = default);
    void AddRange(IEnumerable<TEntity> entities);
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken token = default);

    // Update operations
    void Edit(TEntity entity);
    Task EditAsync(TEntity entity);
    void EditRange(IEnumerable<TEntity> entities);
    Task EditRangeAsync(IEnumerable<TEntity> entities);

    // Read operations
    TEntity? GetById(TKey id);
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken token = default);
    IList<TEntity> GetAll();
    Task<IList<TEntity>> GetAllAsync(CancellationToken token = default);
    Task<IList<TEntity>> GetByConditionAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken token = default);
    Task<IQueryable<TEntity>> Where(Expression<Func<TEntity, bool>> predicate);
    TEntity? FindWhere(Expression<Func<TEntity, bool>> predicate);
    Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken token = default);
    Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
    Task<TEntity?> SingleAsync(Expression<Func<TEntity, bool>> predicate);

    // Count operations
    int GetCount(Expression<Func<TEntity, bool>>? filter = null);
    Task<int> GetCountAsync(Expression<Func<TEntity, bool>>? filter = null);
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken token = default);
    Task<int> GetDistinctCountAsync<TProperty>(
        Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TProperty>> selector,
        CancellationToken token = default);

    // Remove operations
    void Remove(TKey id);
    void Remove(TEntity entity);
    void Remove(Expression<Func<TEntity, bool>> filter);
    void RemoveRange(IEnumerable<TEntity> entities);
    Task RemoveAsync(TKey id);
    Task RemoveAsync(TEntity entity);
    Task RemoveAsync(Expression<Func<TEntity, bool>> filter);
    Task RemoveRangeAsync(IEnumerable<TEntity> entities);
    Task RemoveRangeConditionalAsync(Func<TEntity, bool> predicate);

    // Soft delete operations
    void SoftDelete(TKey id);
    void SoftDelete(TEntity entity);
    void SoftDelete(Expression<Func<TEntity, bool>> filter);
    Task SoftDeleteAsync(TKey id);
    Task SoftDeleteAsync(TEntity entity);
    Task SoftDeleteAsync(Expression<Func<TEntity, bool>> filter);

    // Save operations
    int Save();
    Task<int> SaveAsync();

    // Pagination
    Task<(IList<TEntity> data, int total, int totalDisplay)> GetPagedAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        int pageIndex = 1,
        int pageSize = 10,
        bool isTrackingOff = false);
}
