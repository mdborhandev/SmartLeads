using Microsoft.EntityFrameworkCore;
using SmartLeads.Domain.DTOs;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Persistence;
using SmartLeads.Infrastructure.Repositories.Interface;

namespace SmartLeads.Infrastructure.Repositories.Implementation;

public class CompanyRepository : BaseRepository<Company, Guid>, ICompanyRepository
{
    public CompanyRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IList<Company>> GetCompaniesByParentIdAsync(Guid? parentId, CancellationToken token = default)
    {
        return await _dbContext.Companies
            .Where(c => c.ParentCompanyId == parentId && !c.IsDeleted)
            .ToListAsync(token);
    }

    public async Task<IList<CompanyDto>> GetCompanyDtosAsync(CancellationToken token = default)
    {
        return await _dbContext.Companies
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
        return await _dbContext.Companies
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
        return await _dbContext.Companies
            .Include(c => c.ChildCompanies)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, token);
    }

    public async Task<Company?> GetByNameAsync(string name)
    {
        return await _dbContext.Companies.FirstOrDefaultAsync(c => c.Name == name && !c.IsDeleted);
    }

    public override async Task<IList<Company>> GetAllAsync(CancellationToken token = default)
    {
        return await _dbContext.Companies.Where(c => !c.IsDeleted).ToListAsync(token);
    }
}
