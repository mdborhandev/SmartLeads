using Microsoft.EntityFrameworkCore;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Persistence;
using SmartLeads.Infrastructure.Repositories.Interface;

namespace SmartLeads.Infrastructure.Repositories.Implementation;

public class EmployeeRepository : GenericRepository<Employee>, IEmployeeRepository
{
    private readonly DefaultDbContext _defaultDbContext;

    public EmployeeRepository(DefaultDbContext dbContext) : base(dbContext)
    {
        _defaultDbContext = dbContext;
    }

    public async Task<Employee?> GetByEmployeeIdAsync(string employeeId, Guid companyId)
    {
        return await _defaultDbContext.Employees
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.CompanyId == companyId && !e.IsDeleted);
    }

    public async Task<Employee?> GetByEmployeeIdExcludingIdAsync(string employeeId, Guid companyId, Guid excludeId)
    {
        return await _defaultDbContext.Employees
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.CompanyId == companyId && e.Id != excludeId && !e.IsDeleted);
    }
}