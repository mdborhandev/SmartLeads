using SmartLeads.Domain.Models;

namespace SmartLeads.Infrastructure.Repositories.Interface;

public interface IVariableRepository : IGenericRepository<Variable>
{
    Task<IEnumerable<Variable>> GetByTypeAsync(string type, Guid companyId);
    Task<IEnumerable<string>> GetTypesAsync(Guid companyId);
    Task<Variable?> GetByTypeAndValueAsync(string type, string value, Guid companyId);
}