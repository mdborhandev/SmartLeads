using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SmartLeads.Infrastructure.Persistence;

public class SystemDbContextDesignTimeFactory : IDesignTimeDbContextFactory<SystemDbContext>
{
    public SystemDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SystemDbContext>();
        
        // Use environment variable or default connection string
        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") 
            ?? "Host=localhost;Port=5432;Database=SystemDbSmartLeads;Username=borhanuddin;Password=borhan444";
        
        optionsBuilder.UseNpgsql(connectionString);
        
        return new SystemDbContext(optionsBuilder.Options);
    }
}
