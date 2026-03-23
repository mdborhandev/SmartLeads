using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SmartLeads.Infrastructure.Persistence;
using SmartLeads.Infrastructure.Repositories;
using SmartLeads.Infrastructure.Repositories.Implementation;
using SmartLeads.Infrastructure.Repositories.Interface;
using SmartLeads.Infrastructure.Services;
using SmartLeads.Infrastructure.Services.Implementation;
using SmartLeads.Infrastructure.Services.Interface;

namespace SmartLeads.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register Memory Cache
        services.AddMemoryCache();
        
        // Register HTTP Context Accessor (needed for CompanyContext)
        services.AddHttpContextAccessor();

        // Register System DbContext (for Users, Companies, UserCompanies, Invitations)
        services.AddDbContext<SystemDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("SystemConnection")));

        // Register Company DbContext (for company-specific data)
        // Note: Connection string will be resolved per-company at runtime
        services.AddDbContext<CompanyDbContext>(options =>
            options.UseNpgsql(GetCompanyConnectionString(configuration)));

        // Register Company Context Factory
        services.AddScoped<ICompanyDbContextFactory, CompanyDbContextFactory>();

        // Register Company Context (tracks current user's company)
        services.AddScoped<ICompanyContext, CompanyContext>();

        // Register generic repository for backward compatibility
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IContactRepository, ContactRepository>();
        services.AddScoped<IInvitationRepository, InvitationRepository>();
        services.AddScoped<IColumnFilterRepository, ColumnFilterRepository>();

        // Register Services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IInvitationService, InvitationService>();

        // JWT Authentication
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not found");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secret))
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    context.Token = context.Request.Cookies["JwtToken"];
                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }

    private static string GetCompanyConnectionString(IConfiguration configuration)
    {
        // Use CompanyConnection string (SmartLeadsDb - contains company-specific data)
        return configuration.GetConnectionString("CompanyConnection")
               ?? throw new InvalidOperationException("Company connection string not found");
    }
}
