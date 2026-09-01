using System.Reflection;
using FluentValidation;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Security;

namespace SLCDM.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        var typeAdapterConfig = TypeAdapterConfig.GlobalSettings;
        typeAdapterConfig.Scan(assembly);
        services.AddSingleton(typeAdapterConfig);
        services.AddScoped<IMapper, ServiceMapper>();
        services.AddSingleton<IPasswordHashService, PasswordHashService>();
        services.AddSingleton<IPasswordGenerator, PasswordGenerator>();

        services.AddValidatorsFromAssembly(assembly);
        RegisterHandlers(services, assembly);

        return services;
    }

    private static void RegisterHandlers(IServiceCollection services, Assembly assembly)
    {
        var implementations = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false, IsPublic: true });

        foreach (var implementation in implementations)
        {
            foreach (var service in implementation.GetInterfaces())
            {
                if (!service.IsGenericType)
                {
                    continue;
                }

                var definition = service.GetGenericTypeDefinition();
                if (definition == typeof(IQueryHandler<,>)
                    || definition == typeof(ICommandHandler<>)
                    || definition == typeof(ICommandHandler<,>))
                {
                    services.AddScoped(service, implementation);
                }
            }
        }
    }
}
