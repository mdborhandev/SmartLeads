using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SmartLeads.Infrastructure.Persistence;

public class SmartLeadsDbContextDesignTimeFactory : IDesignTimeDbContextFactory<SmartLeadsDbContext>
{
    public SmartLeadsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SmartLeadsDbContext>();
        
        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") 
            ?? "Host=localhost;Port=5432;Database=SmartLeadsCoreDb;Username=borhanuddin;Password=borhan444";
        
        optionsBuilder.UseNpgsql(connectionString);
        
        return new SmartLeadsDbContext(optionsBuilder.Options);
    }
}
