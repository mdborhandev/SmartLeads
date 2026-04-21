using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Repositories.Interface;

namespace SmartLeads.Infrastructure.Repositories.Interface;

public interface IEmployeeRepository : IBaseRepository<Employee, Guid>
{
    Task<Employee?> GetByEmployeeIdAsync(string employeeId, Guid companyId);
    Task<Employee?> GetByEmployeeIdExcludingIdAsync(string employeeId, Guid companyId, Guid excludeId);
}