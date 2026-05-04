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

        // Register Default DbContext (for contact and related application data)
        services.AddDbContext<DefaultDbContext>(options =>
            options.UseNpgsql(GetDefaultConnectionString(configuration)));

        // Register default DbContext factory
        services.AddScoped<IDefaultDbContextFactory, DefaultDbContextFactory>();

        // Register Company Context (tracks current user's company)
        services.AddScoped<ICompanyContext, CompanyContext>();

        // Register generic repository for backward compatibility
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<SmartLeads.Utilities.Interfaces.IUserRepository>(sp => sp.GetRequiredService<IUserRepository>());
        
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IContactRepository, ContactRepository>();
        services.AddScoped<IInvitationRepository, InvitationRepository>();
        services.AddScoped<IColumnFilterRepository, ColumnFilterRepository>();
        
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<SmartLeads.Utilities.Interfaces.INotificationRepository>(sp => sp.GetRequiredService<INotificationRepository>());
        
        services.AddScoped<INotificationPreferenceRepository, NotificationPreferenceRepository>();
        services.AddScoped<SmartLeads.Utilities.Interfaces.INotificationPreferenceRepository>(sp => sp.GetRequiredService<INotificationPreferenceRepository>());

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

    private static string GetDefaultConnectionString(IConfiguration configuration)
    {
        return configuration.GetConnectionString("DefaultConnection")
               ?? throw new InvalidOperationException("Default connection string not found");
    }
}
