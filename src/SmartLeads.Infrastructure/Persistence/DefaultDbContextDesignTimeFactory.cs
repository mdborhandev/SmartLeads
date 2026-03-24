using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SmartLeads.Infrastructure.Persistence;

public class DefaultDbContextDesignTimeFactory : IDesignTimeDbContextFactory<DefaultDbContext>
{
    public DefaultDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DefaultDbContext>();
        
        // Use environment variable or default connection string
        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") 
            ?? "Host=localhost;Port=5432;Database=SmartLeadsAppDb;Username=borhanuddin;Password=borhan444";
        
        optionsBuilder.UseNpgsql(connectionString);
        
        return new DefaultDbContext(optionsBuilder.Options);
    }
}
