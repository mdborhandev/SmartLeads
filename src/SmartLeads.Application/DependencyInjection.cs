using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;

namespace SmartLeads.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(configuration => 
            configuration.RegisterServicesFromAssembly(assembly));

        services.AddAutoMapper(cfg => cfg.AddMaps(assembly));
        
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
