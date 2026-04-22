using Microsoft.EntityFrameworkCore;
using SmartLeads.Domain.DTOs;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Persistence;
using SmartLeads.Infrastructure.Repositories.Interface;

namespace SmartLeads.Infrastructure.Repositories.Implementation;

public class VariableRepository : GenericRepository<Variable>, IVariableRepository
{
    private readonly DefaultDbContext _defaultDbContext;

    public VariableRepository(DefaultDbContext dbContext) : base(dbContext)
    {
        _defaultDbContext = dbContext;
    }

    private readonly List<CommonDataTypeDto> _commonDataTypes = new List<CommonDataTypeDto>
    {
        new CommonDataTypeDto { Id = "Gender", Text = "Gender" },
        new CommonDataTypeDto { Id = "Marital Status", Text = "Marital Status" },
        new CommonDataTypeDto { Id = "Blood Group", Text = "Blood Group" },
        new CommonDataTypeDto { Id = "Nationality", Text = "Nationality" },
        new CommonDataTypeDto { Id = "Joining Type", Text = "Joining Type" },
        new CommonDataTypeDto { Id = "Employment Status", Text = "Employment Status" }
    };

    public List<CommonDataTypeDto> GetAllCommonDataType(string searchTerm = null)
    {
        var result = _commonDataTypes;

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            result = result
                .Where(x => x.Text.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return result.OrderBy(x => x.Text).ToList();
    }

    public List<CommonDataTypeDto> GetAllCommonDataType(string searchTerm, string selectedvalue)
    {
        var result = _commonDataTypes.Select(x => new CommonDataTypeDto
        {
            Id = x.Id,
            Text = x.Text,
            selected = selectedvalue != "" && x.Id == selectedvalue
        }).ToList();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            result = result
                .Where(x => x.Text.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return result.OrderBy(x => x.Text).ToList();
    }

    public async Task<IEnumerable<Variable>> GetByTypeAsync(string type, Guid companyId)
    {
        return await _defaultDbContext.Variables
            .Where(v => v.Type == type && v.CompanyId == companyId && !v.IsDeleted)
            .OrderBy(v => v.SortOrder)
            .ThenBy(v => v.Value)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetTypesAsync(Guid companyId)
    {
        return await _defaultDbContext.Variables
            .Where(v => v.CompanyId == companyId && !v.IsDeleted)
            .Select(v => v.Type)
            .Distinct()
            .OrderBy(v => v)
            .ToListAsync();
    }

    public async Task<Variable?> GetByTypeAndValueAsync(string type, string value, Guid companyId)
    {
        return await _defaultDbContext.Variables
            .FirstOrDefaultAsync(v => v.Type == type && v.Value == value && v.CompanyId == companyId && !v.IsDeleted);
    }
}