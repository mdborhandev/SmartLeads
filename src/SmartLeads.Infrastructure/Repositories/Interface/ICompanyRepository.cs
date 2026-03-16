using SmartLeads.Domain.DTOs;
using SmartLeads.Domain.Models;

namespace SmartLeads.Infrastructure.Repositories.Interface;

public interface ICompanyRepository
{
    Task<IList<Company>> GetAllAsync(CancellationToken token = default);
    Task<Company?> GetByIdAsync(Guid id, CancellationToken token = default);
    Task<Company?> GetCompanyWithChildrenAsync(Guid id, CancellationToken token = default);
    Task<IList<Company>> GetCompaniesByParentIdAsync(Guid? parentId, CancellationToken token = default);
    Task<IList<CompanyDto>> GetCompanyDtosAsync(CancellationToken token = default);
    Task<CompanyDto?> GetCompanyDtoByIdAsync(Guid id, CancellationToken token = default);
    void Add(Company entity);
    Task AddAsync(Company entity, CancellationToken token = default);
    void Edit(Company entity);
    Task EditAsync(Company entity);
    void Remove(Guid id);
    Task RemoveAsync(Guid id);
    Task<int> SaveAsync(CancellationToken token = default);
}
