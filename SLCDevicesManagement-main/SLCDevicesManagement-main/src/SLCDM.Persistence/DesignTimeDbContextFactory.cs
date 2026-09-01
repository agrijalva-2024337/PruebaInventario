using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SLCDM.Persistence;

/// <summary>
/// Design-time para <c>dotnet ef</c>. EF usa esta fabrica y NO el host de
/// SLCDM.Api, asi que un cambio solo en appsettings no basta si el fallback
/// queda en <c>localhost</c>.
/// Orden: variable CONNECTIONSTRINGS__DEFAULTCONNECTION, luego
/// appsettings de SLCDM.Api, luego named pipe de SQLEXPRESS local.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    private const string LocalExpressPipe =
        @"Server=np:\\.\pipe\MSSQL$SQLEXPRESS\sql\query;Database=DercasInventario;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False";

    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(ResolveConnectionString());
        return new ApplicationDbContext(optionsBuilder.Options);
    }

    private static string ResolveConnectionString()
    {
        var fromEnv = Environment.GetEnvironmentVariable("CONNECTIONSTRINGS__DEFAULTCONNECTION");
        if (!string.IsNullOrWhiteSpace(fromEnv))
        {
            return fromEnv;
        }

        var apiDir = FindApiDirectory();
        if (apiDir is not null)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(apiDir)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var fromJson = config.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrWhiteSpace(fromJson))
            {
                return fromJson;
            }
        }

        return LocalExpressPipe;
    }

    private static string? FindApiDirectory()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "src", "SLCDM.Api");
            if (File.Exists(Path.Combine(candidate, "appsettings.json")))
            {
                return candidate;
            }

            dir = dir.Parent;
        }

        return null;
    }
}
