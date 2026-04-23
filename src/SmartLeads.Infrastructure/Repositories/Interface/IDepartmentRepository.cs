using SmartLeads.Domain.DTOs;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Repositories.Interface;

namespace SmartLeads.Infrastructure.Repositories.Interface;

public interface IDepartmentRepository : IGenericRepository<Department>
{
    Task<List<SelectOptionDto>> SearchDepartmentsAsync(string searchTerm, string selectedvalue, Guid companyId);
}
