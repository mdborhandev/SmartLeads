using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SmartLeads.Infrastructure.Persistence;

public class CompanyDbContextDesignTimeFactory : IDesignTimeDbContextFactory<CompanyDbContext>
{
    public CompanyDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CompanyDbContext>();
        
        // Use environment variable or default connection string
        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") 
            ?? "Host=localhost;Port=5432;Database=SmartLeadsDb;Username=borhanuddin;Password=borhan444";
        
        optionsBuilder.UseNpgsql(connectionString);
        
        return new CompanyDbContext(optionsBuilder.Options);
    }
}
