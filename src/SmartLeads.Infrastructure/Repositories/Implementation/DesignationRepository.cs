using SmartLeads.Domain.DTOs;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Persistence;
using SmartLeads.Infrastructure.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace SmartLeads.Infrastructure.Repositories.Implementation;

public class DesignationRepository : GenericRepository<Designation>, IDesignationRepository
{
    public DesignationRepository(DefaultDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<List<SelectOptionDto>> SearchDesignationsAsync(string searchTerm, string selectedvalue, Guid companyId, Guid? departmentId = null)
    {
        var query = _defaultDbContext.Designations
            .Where(d => d.CompanyId == companyId && !d.IsDeleted && d.IsActive);

        if (departmentId.HasValue)
        {
            query = query.Where(d => d.DepartmentId == departmentId.Value);
        }

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
