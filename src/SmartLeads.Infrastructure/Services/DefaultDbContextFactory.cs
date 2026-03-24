using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SmartLeads.Infrastructure.Persistence;

namespace SmartLeads.Infrastructure.Services;

/// <summary>
/// Service for creating default database contexts.
/// </summary>
public interface IDefaultDbContextFactory
{
    /// <summary>
    /// Creates a new DefaultDbContext instance.
    /// </summary>
    DefaultDbContext Create(Guid companyId);

    /// <summary>
    /// Creates a new DefaultDbContext instance using the provided connection string.
    /// </summary>
    DefaultDbContext CreateWithConnectionString(string connectionString);
}

public class DefaultDbContextFactory : IDefaultDbContextFactory
{
    private readonly string _defaultConnectionString;

    public DefaultDbContextFactory(IConfiguration configuration)
    {
        _defaultConnectionString = configuration.GetConnectionString("DefaultConnection")
                                   ?? throw new InvalidOperationException("Default connection string not found");
    }

    public DefaultDbContext Create(Guid companyId)
    {
        return CreateWithConnectionString(_defaultConnectionString);
    }

    public DefaultDbContext CreateWithConnectionString(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DefaultDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        return new DefaultDbContext(optionsBuilder.Options);
    }
}
