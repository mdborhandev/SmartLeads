using SmartLeads.Domain.DTOs;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Repositories.Interface;

namespace SmartLeads.Infrastructure.Repositories.Interface;

public interface IDesignationRepository : IGenericRepository<Designation>
{
    Task<List<SelectOptionDto>> SearchDesignationsAsync(string searchTerm, string selectedvalue, Guid companyId, Guid? departmentId = null);
}
