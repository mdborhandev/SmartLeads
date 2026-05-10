using SmartLeads.Domain.DTOs;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Persistence;
using SmartLeads.Infrastructure.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace SmartLeads.Infrastructure.Repositories.Implementation;

public class DepartmentRepository : GenericRepository<Department>, IDepartmentRepository
{
    private new readonly SmartLeadsDbContext _dbContext;

    public DepartmentRepository(SmartLeadsDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<SelectOptionDto>> SearchDepartmentsAsync(string searchTerm, string selectedvalue, Guid companyId)
    {
        var query = _dbContext.Departments
            .Where(d => d.CompanyId == companyId && !d.IsDeleted && d.IsActive);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchLower = searchTerm.ToLower();
            query = query.Where(d => d.Name.ToLower().Contains(searchLower));
        }

        return await query
            .OrderBy(d => d.Name)
            .Take(20)
            .Select(d => new SelectOptionDto
            {
                id = d.Id.ToString(),
                text = d.Name,
                selected = selectedvalue != "" && d.Id.ToString() == selectedvalue
            })
            .ToListAsync();
    }
}
