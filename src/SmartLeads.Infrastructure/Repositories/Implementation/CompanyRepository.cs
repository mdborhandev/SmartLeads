using Microsoft.EntityFrameworkCore;
using SmartLeads.Domain.DTOs;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Persistence;
using SmartLeads.Infrastructure.Repositories.Interface;

namespace SmartLeads.Infrastructure.Repositories.Implementation;

public class CompanyRepository : ICompanyRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DbSet<Company> _dbSet;

    public CompanyRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = _dbContext.Companies;
    }

    public virtual void Add(Company entity)
    {
        _dbSet.Add(entity);
    }

    public virtual async Task AddAsync(Company entity, CancellationToken token = default)
    {
        await _dbSet.AddAsync(entity, token);
    }

    public virtual void Edit(Company entity)
    {
        _dbSet.Attach(entity);
        _dbContext.Entry(entity).State = EntityState.Modified;
    }

    public virtual async Task EditAsync(Company entity)
    {
        _dbSet.Attach(entity);
        _dbContext.Entry(entity).State = EntityState.Modified;
    }

    public virtual async Task<Company?> GetByIdAsync(Guid id, CancellationToken token = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, token);
    }

    public virtual async Task<IList<Company>> GetAllAsync(CancellationToken token = default)
    {
        return await _dbSet.Where(c => !c.IsDeleted).ToListAsync(token);
    }

    public async Task<IList<Company>> GetCompaniesByParentIdAsync(Guid? parentId, CancellationToken token = default)
    {
        return await _dbSet
            .Where(c => c.ParentCompanyId == parentId && !c.IsDeleted)
            .ToListAsync(token);
    }

    public async Task<IList<CompanyDto>> GetCompanyDtosAsync(CancellationToken token = default)
    {
        return await _dbSet
            .Where(c => !c.IsDeleted)
            .Select(c => new CompanyDto(
                c.Id,
                c.Name,
                c.Code,
                c.Address,
                c.Phone,
                c.Email,
                c.Logo,
                c.IsActive,
                c.IsParent,
                c.ParentCompanyId,
                c.ParentCompany != null ? c.ParentCompany.Name : null,
                c.CreatedAt))
            .ToListAsync(token);
    }

    public async Task<CompanyDto?> GetCompanyDtoByIdAsync(Guid id, CancellationToken token = default)
    {
        return await _dbSet
            .Where(c => c.Id == id && !c.IsDeleted)
            .Select(c => new CompanyDto(
                c.Id,
                c.Name,
                c.Code,
                c.Address,
                c.Phone,
                c.Email,
                c.Logo,
                c.IsActive,
                c.IsParent,
                c.ParentCompanyId,
                c.ParentCompany != null ? c.ParentCompany.Name : null,
                c.CreatedAt))
            .FirstOrDefaultAsync(token);
    }

    public async Task<Company?> GetCompanyWithChildrenAsync(Guid id, CancellationToken token = default)
    {
        return await _dbSet
            .Include(c => c.ChildCompanies)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, token);
    }

    public virtual void Remove(Guid id)
    {
        var entity = _dbSet.Find(id);
        if (entity != null)
            _dbSet.Remove(entity);
    }

    public virtual async Task RemoveAsync(Guid id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity != null)
            _dbSet.Remove(entity);
    }

    public virtual async Task<int> SaveAsync(CancellationToken token = default)
    {
        return await _dbContext.SaveChangesAsync(token);
    }
}
