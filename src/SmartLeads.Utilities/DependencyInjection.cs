using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartLeads.Utilities.Email;
using SmartLeads.Utilities.Identity;
using SmartLeads.Utilities.Interfaces;

namespace SmartLeads.Utilities;

public static class DependencyInjection
{
    public static IServiceCollection AddUtilities(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}
