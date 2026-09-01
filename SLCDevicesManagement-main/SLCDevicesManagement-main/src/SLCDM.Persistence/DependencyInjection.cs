using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SLCDM.Application.Common.Interfaces;

namespace SLCDM.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=.\\SQLEXPRESS;Database=DercasInventario;Trusted_Connection=True;TrustServerCertificate=True;";

        services.AddScoped<Interceptors.BitacoraSaveChangesInterceptor>();
        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString);
            options.AddInterceptors(sp.GetRequiredService<Interceptors.BitacoraSaveChangesInterceptor>());
        });

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<ApplicationDbContext>());

        return services;
    }
}
