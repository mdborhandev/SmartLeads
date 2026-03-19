using SmartLeads.Domain.Models;

namespace SmartLeads.Infrastructure.Repositories.Interface;

public interface IColumnFilterRepository : IBaseRepository<ColumnFilter, Guid>
{
    Task<ColumnFilter?> GetColumnFilterByUserAndListNameAsync(Guid userId, Guid? companyId, string listName, CancellationToken token = default);
}
