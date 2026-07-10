/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using NexumAltivon.API.ERP.SharedData;
using NexumAltivon.API.Infrastructure.Tenancy;

namespace NexumAltivon.API.Data;

public sealed class NexumDbContextDesignTimeFactory : IDesignTimeDbContextFactory<NexumDbContext>
{
    public NexumDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<NexumDbContext>()
            .UseMySql(
                DesignTimeConnectionStrings.Resolve("DefaultConnection", "NexumDb", "nexum_altivon"),
                new MySqlServerVersion(new Version(8, 0, 0)))
            .Options;

        return new NexumDbContext(options, new TenantContext());
    }
}

public sealed class GenesisDbContextDesignTimeFactory : IDesignTimeDbContextFactory<GenesisDbContext>
{
    public GenesisDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<GenesisDbContext>()
            .UseMySql(
                DesignTimeConnectionStrings.Resolve("GenesisConnection", null, "genesis_bd"),
                new MySqlServerVersion(new Version(8, 0, 0)))
            .Options;

        return new GenesisDbContext(options);
    }
}

internal static class DesignTimeConnectionStrings
{
    public static string Resolve(string primaryName, string? secondaryName, string localDatabaseName)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(ResolveProjectRoot())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile(Path.Combine("API", "appsettings.json"), optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var configured = configuration.GetConnectionString(primaryName);
        if (string.IsNullOrWhiteSpace(configured) && !string.IsNullOrWhiteSpace(secondaryName))
        {
            configured = configuration.GetConnectionString(secondaryName);
        }

        return IsUsableConnectionString(configured)
            ? configured!.Trim()
            : BuildLocalXamppConnectionString(localDatabaseName);
    }

    private static string BuildLocalXamppConnectionString(string databaseName)
    {
        return $"Server=127.0.0.1;Port=3309;Database={databaseName};Uid=root;Pwd=;SslMode=none;AllowPublicKeyRetrieval=true;";
    }

    private static bool IsUsableConnectionString(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = value.Trim().ToUpperInvariant();
        var invalidMarkers = new[]
        {
            "CHANGE" + "_ME",
            "PREEN" + "CHER",
            "USE" + "_ENV",
            "SUA" + "_SENHA"
        };

        return !invalidMarkers.Any(normalized.Contains);
    }

    private static string ResolveProjectRoot()
    {
        var current = Directory.GetCurrentDirectory();
        var directory = new DirectoryInfo(current);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "NexumAltivon.API.csproj")))
            {
                return directory.FullName;
            }

            var backendProject = Path.Combine(directory.FullName, "NexumAltivon_Back-End", "NexumAltivon.API.csproj");
            if (File.Exists(backendProject))
            {
                return Path.Combine(directory.FullName, "NexumAltivon_Back-End");
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Diretorio do projeto NexumAltivon.API.csproj nao localizado para design-time EF Core.");
    }
}
