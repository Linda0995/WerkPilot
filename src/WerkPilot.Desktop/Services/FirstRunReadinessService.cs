using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using WerkPilot.Infrastructure.Persistence;

namespace WerkPilot.Desktop.Services;

public sealed class FirstRunReadinessService(
    IConfiguration configuration,
    IHostEnvironment hostEnvironment,
    IDbContextFactory<WerkPilotDbContext> dbContextFactory)
{
    public async Task<FirstRunReadinessSnapshot> CheckAsync(
        CancellationToken cancellationToken = default)
    {
        var connection =
            configuration.GetConnectionString("WerkPilot")
            ?? Environment.GetEnvironmentVariable(
                "ConnectionStrings__WerkPilot")
            ?? Environment.GetEnvironmentVariable(
                "WERKPILOT_CONNECTION_STRING");

        var adminBootstrap =
            !string.IsNullOrWhiteSpace(
                Environment.GetEnvironmentVariable(
                    "WERKPILOT_ADMIN_INITIAL_PASSWORD"));

        var demoMode =
            bool.TryParse(
                Environment.GetEnvironmentVariable(
                    "WERKPILOT_SEED_DEMO_DATA"),
                out var enabled)
            && enabled;

        var databaseConfigured =
            !string.IsNullOrWhiteSpace(connection);

        var databaseReachable = false;

        if (databaseConfigured)
        {
            try
            {
                await using var db =
                    await dbContextFactory.CreateDbContextAsync(
                        cancellationToken);

                databaseReachable =
                    await db.Database.CanConnectAsync(
                        cancellationToken);
            }
            catch
            {
                databaseReachable = false;
            }
        }

        var version =
            typeof(FirstRunReadinessService)
                .Assembly
                .GetName()
                .Version?
                .ToString()
            ?? "unbekannt";

        var status = ResolveStatus(
            databaseConfigured,
            databaseReachable,
            adminBootstrap);

        return new FirstRunReadinessSnapshot(
            databaseConfigured,
            databaseReachable,
            adminBootstrap,
            demoMode,
            hostEnvironment.EnvironmentName,
            version,
            status,
            DateTimeOffset.UtcNow);
    }

    private static string ResolveStatus(
        bool databaseConfigured,
        bool databaseReachable,
        bool adminBootstrapConfigured)
    {
        if (!databaseConfigured)
            return "Datenbankverbindung fehlt.";

        if (!databaseReachable)
            return "Datenbank ist nicht erreichbar.";

        if (!adminBootstrapConfigured)
            return "Admin-Erstkennwort ist noch nicht gesetzt.";

        return "WerkPilot ist für den Erststart bereit.";
    }
}
