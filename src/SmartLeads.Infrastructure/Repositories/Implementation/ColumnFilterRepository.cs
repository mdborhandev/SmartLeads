using Microsoft.EntityFrameworkCore;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Persistence;
using SmartLeads.Infrastructure.Repositories.Interface;

namespace SmartLeads.Infrastructure.Repositories.Implementation;

public class ColumnFilterRepository : BaseRepository<ColumnFilter, Guid>, IColumnFilterRepository
{
    private readonly DefaultDbContext _defaultDbContext;

    public ColumnFilterRepository(DefaultDbContext dbContext) : base(dbContext)
    {
        _defaultDbContext = dbContext;
    }

    public async Task<ColumnFilter?> GetColumnFilterByUserAndListNameAsync(Guid userId, Guid? companyId, string listName, CancellationToken token = default)
    {
        return await SingleOrDefaultAsync(cf => cf.CreatedByUserId == userId
                                     && cf.ListName == listName, token);
    }
}
