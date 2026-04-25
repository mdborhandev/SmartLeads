using SmartLeads.Domain.DTOs;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Repositories.Interface;

namespace SmartLeads.Infrastructure.Repositories.Interface;

public interface IEmployeeRepository : IBaseRepository<Employee, Guid>
{
    Task<Employee?> GetByEmployeeIdAsync(string employeeId, Guid companyId);
    Task<Employee?> GetByEmployeeIdExcludingIdAsync(string employeeId, Guid companyId, Guid excludeId);
    Task<Employee?> GetByEmployeeDtoByIdAsync(Guid id);
    Task<(List<EmployeeDto> data, int total)> GetEmployeesDataAsync(string searchTerm, string sortField, string sortOrder, int page, int pageSize, Guid companyId);
}