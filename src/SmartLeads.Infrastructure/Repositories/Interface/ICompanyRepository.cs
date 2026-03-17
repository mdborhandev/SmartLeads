using SmartLeads.Domain.DTOs;
using SmartLeads.Domain.Models;

namespace SmartLeads.Infrastructure.Repositories.Interface;

public interface ICompanyRepository : IBaseRepository<Company, Guid>
{
    Task<Company?> GetCompanyWithChildrenAsync(Guid id, CancellationToken token = default);
    Task<IList<Company>> GetCompaniesByParentIdAsync(Guid? parentId, CancellationToken token = default);
    Task<IList<CompanyDto>> GetCompanyDtosAsync(CancellationToken token = default);
    Task<CompanyDto?> GetCompanyDtoByIdAsync(Guid id, CancellationToken token = default);
    Task<Company?> GetByNameAsync(string name);
}
