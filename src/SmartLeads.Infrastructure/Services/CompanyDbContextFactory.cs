using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using SmartLeads.Infrastructure.Persistence;

namespace SmartLeads.Infrastructure.Services;

/// <summary>
/// Service for managing company-specific database connections.
/// Each company has its own database, and this service helps switch between them.
/// </summary>
public interface ICompanyDbContextFactory
{
    /// <summary>
    /// Creates a new CompanyDbContext instance for the specified company.
    /// </summary>
    CompanyDbContext Create(Guid companyId);

    /// <summary>
    /// Creates a new CompanyDbContext instance using the provided connection string.
    /// </summary>
    CompanyDbContext CreateWithConnectionString(string connectionString);
}

public class CompanyDbContextFactory : ICompanyDbContextFactory
{
    private readonly string _defaultConnectionString;

    public CompanyDbContextFactory(IConfiguration configuration)
    {
        _defaultConnectionString = configuration.GetConnectionString("CompanyConnection") 
                                   ?? configuration.GetConnectionString("DefaultConnection")
                                   ?? throw new InvalidOperationException("Company connection string not found");
    }

    public CompanyDbContext Create(Guid companyId)
    {
        // In a multi-tenant setup, you would construct the connection string per company
        // For example: Company_{companyId} or from a configuration store
        var connectionString = BuildCompanyConnectionString(companyId);
        return CreateWithConnectionString(connectionString);
    }

    public CompanyDbContext CreateWithConnectionString(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CompanyDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        return new CompanyDbContext(optionsBuilder.Options);
    }

    private string BuildCompanyConnectionString(Guid companyId)
    {
        // Strategy 1: Different database per company (same server)
        // Modify the database name in the connection string
        var builder = new NpgsqlConnectionStringBuilder(_defaultConnectionString)
        {
            Database = $"SmartLeads_Company_{companyId:N}"
        };
        return builder.ConnectionString;

        // Strategy 2: Completely different server per company (uncomment if needed)
        // return $"Host=company-{companyId}.server.com;Database=SmartLeads;Username=...;Password=...";
    }
}
