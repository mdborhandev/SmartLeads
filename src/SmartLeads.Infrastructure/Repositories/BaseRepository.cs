using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Persistence;
using SmartLeads.Infrastructure.Repositories.Interface;
using System.Linq.Expressions;

namespace SmartLeads.Infrastructure.Repositories;

public abstract class BaseRepository<TEntity, TKey> : IBaseRepository<TEntity, TKey>
    where TEntity : class
{
    protected readonly ApplicationDbContext _dbContext;
    private readonly DbSet<TEntity> _dbSet;
    protected int CommandTimeout { get; set; }

    public BaseRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = _dbContext.Set<TEntity>();
        CommandTimeout = 300;
    }

    public virtual void Add(TEntity entityToAdd)
    {
        _dbSet.Add(entityToAdd);
    }

    public virtual async Task AddAsync(TEntity entityToAdd, CancellationToken token = default)
    {
        await _dbSet.AddAsync(entityToAdd, token);
    }

    public virtual void AddRange(IEnumerable<TEntity> entities)
    {
        if (entities == null || !entities.Any())
            throw new ArgumentNullException(nameof(entities));

        _dbSet.AddRange(entities);
    }

    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken token = default)
    {
        if (entities == null || !entities.Any())
            throw new ArgumentNullException(nameof(entities));

        await _dbSet.AddRangeAsync(entities, token);
    }

    public virtual void Edit(TEntity entityToEdit)
    {
        _dbSet.Attach(entityToEdit);
        _dbContext.Entry(entityToEdit).State = EntityState.Modified;
    }

    public virtual async Task EditAsync(TEntity entityToEdit)
    {
        await Task.Run(() =>
        {
            _dbSet.Attach(entityToEdit);
            _dbContext.Entry(entityToEdit).State = EntityState.Modified;
        });
    }

    public virtual void EditRange(IEnumerable<TEntity> entities)
    {
        if (entities == null || !entities.Any())
            throw new ArgumentNullException(nameof(entities));

        foreach (var entity in entities)
        {
            _dbSet.Update(entity);
        }
    }

    public virtual async Task EditRangeAsync(IEnumerable<TEntity> entities)
    {
        if (entities == null || !entities.Any())
            throw new ArgumentNullException(nameof(entities));

        foreach (var entity in entities)
        {
            await Task.Run(() =>
            {
                _dbSet.Update(entity);
            });
        }
    }

    public virtual TEntity? GetById(TKey id)
    {
        return _dbSet.Find(id);
    }

    public virtual async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken token = default)
    {
        return await _dbSet.FindAsync(new object[] { id! }, token);
    }

    public virtual IList<TEntity> GetAll()
    {
        return _dbSet.ToList();
    }

    public virtual async Task<IList<TEntity>> GetAllAsync(CancellationToken token = default)
    {
        return await _dbSet.ToListAsync(token);
    }

    public virtual async Task<IList<TEntity>> GetByConditionAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken token = default)
    {
        return await _dbSet.Where(predicate).ToListAsync(token);
    }

    public virtual async Task<IQueryable<TEntity>> Where(Expression<Func<TEntity, bool>> predicate)
    {
        return await Task.FromResult(_dbSet.Where(predicate).AsQueryable());
    }

    public virtual TEntity? FindWhere(Expression<Func<TEntity, bool>> predicate)
    {
        return _dbSet.Where(predicate).AsNoTracking().FirstOrDefault();
    }

    public virtual async Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken token = default)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate, token);
    }

    public virtual async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public virtual async Task<TEntity?> SingleAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.SingleOrDefaultAsync(predicate);
    }

    public virtual int GetCount(Expression<Func<TEntity, bool>>? filter = null)
    {
        IQueryable<TEntity> query = _dbSet;
        return filter != null ? query.Count(filter) : query.Count();
    }

    public virtual async Task<int> GetCountAsync(Expression<Func<TEntity, bool>>? filter = null)
    {
        IQueryable<TEntity> query = _dbSet;
        return filter != null ? await query.CountAsync(filter) : await query.CountAsync();
    }

    public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken token = default)
    {
        return await _dbSet.AnyAsync(predicate, token);
    }

    public virtual async Task<int> GetDistinctCountAsync<TProperty>(
        Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TProperty>> selector,
        CancellationToken token = default)
    {
        IQueryable<TEntity> query = _dbSet;
        if (filter != null)
        {
            query = query.Where(filter);
        }

        return await query.Select(selector).Distinct().CountAsync(token);
    }

    public virtual void Remove(TKey id)
    {
        var entityToDelete = _dbSet.Find(id);
        if (entityToDelete != null)
            Remove(entityToDelete);
    }

    public virtual void Remove(TEntity entityToRemove)
    {
        if (_dbContext.Entry(entityToRemove).State == EntityState.Detached)
        {
            _dbSet.Attach(entityToRemove);
        }
        _dbSet.Remove(entityToRemove);
    }

    public virtual void Remove(Expression<Func<TEntity, bool>> filter)
    {
        _dbSet.RemoveRange(_dbSet.Where(filter));
    }

    public virtual void RemoveRange(IEnumerable<TEntity> entities)
    {
        if (entities == null || !entities.Any())
            throw new ArgumentNullException(nameof(entities));

        _dbSet.RemoveRange(entities);
    }

    public virtual async Task RemoveAsync(TKey id)
    {
        var entityToRemove = _dbSet.Find(id);
        if (entityToRemove != null)
            await RemoveAsync(entityToRemove);
    }

    public virtual async Task RemoveAsync(TEntity entityToRemove)
    {
        await Task.Run(() =>
        {
            if (_dbContext.Entry(entityToRemove).State == EntityState.Detached)
            {
                _dbSet.Attach(entityToRemove);
            }
            _dbSet.Remove(entityToRemove);
        });
    }

    public virtual async Task RemoveAsync(Expression<Func<TEntity, bool>> filter)
    {
        await Task.Run(() =>
        {
            _dbSet.RemoveRange(_dbSet.Where(filter));
        });
    }

    public virtual async Task RemoveRangeAsync(IEnumerable<TEntity> entities)
    {
        if (entities == null || !entities.Any())
            throw new ArgumentNullException(nameof(entities));

        await Task.Run(() => _dbSet.RemoveRange(entities));
    }

    public virtual async Task RemoveRangeConditionalAsync(Func<TEntity, bool> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        var entities = _dbSet.Where(predicate).ToList();
        if (entities == null || !entities.Any())
            throw new InvalidOperationException("No entities found to remove.");

        await Task.Run(() => _dbSet.RemoveRange(entities));
    }

    public virtual void SoftDelete(TKey id)
    {
        var entity = _dbSet.Find(id);
        if (entity != null)
        {
            SoftDelete(entity);
        }
    }

    public virtual void SoftDelete(TEntity entity)
    {
        if (entity is BaseModel baseModel)
        {
            baseModel.IsDeleted = true;
            baseModel.DeletedAt = DateTime.UtcNow;
            _dbContext.Entry(entity).State = EntityState.Modified;
        }
    }

    public virtual void SoftDelete(Expression<Func<TEntity, bool>> filter)
    {
        var entities = _dbSet.Where(filter).ToList();
        foreach (var entity in entities)
        {
            SoftDelete(entity);
        }
    }

    public virtual async Task SoftDeleteAsync(TKey id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity != null)
        {
            await SoftDeleteAsync(entity);
        }
    }

    public virtual async Task SoftDeleteAsync(TEntity entity)
    {
        await Task.Run(() =>
        {
            if (entity is BaseModel baseModel)
            {
                baseModel.IsDeleted = true;
                baseModel.DeletedAt = DateTime.UtcNow;
                _dbContext.Entry(entity).State = EntityState.Modified;
            }
        });
    }

    public virtual async Task SoftDeleteAsync(Expression<Func<TEntity, bool>> filter)
    {
        var query = _dbSet.Where(filter);
        await query.ExecuteUpdateAsync(setters => setters
            .SetProperty(e => (e as BaseModel)!.IsDeleted, true)
            .SetProperty(e => (e as BaseModel)!.DeletedAt, _ => DateTime.UtcNow));
    }

    public virtual int Save()
    {
        return _dbContext.SaveChanges();
    }

    public virtual async Task<int> SaveAsync()
    {
        return await _dbContext.SaveChangesAsync();
    }

    public virtual async Task<(IList<TEntity> data, int total, int totalDisplay)> GetPagedAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        int pageIndex = 1,
        int pageSize = 10,
        bool isTrackingOff = false)
    {
        IQueryable<TEntity> query = _dbSet;
        var total = await query.CountAsync();
        var totalDisplay = total;

        if (filter != null)
        {
            query = query.Where(filter);
            totalDisplay = await query.CountAsync();
        }

        if (include != null)
            query = include(query);

        if (orderBy != null)
        {
            query = orderBy(query);
        }

        query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);

        if (isTrackingOff)
        {
            query = query.AsNoTracking();
        }

        var data = await query.ToListAsync();
        return (data, total, totalDisplay);
    }
}
