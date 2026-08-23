using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace WerkPilot.Desktop.Services;

public sealed class ReleaseDiagnosticsService(
    IConfiguration configuration,
    IHostEnvironment hostEnvironment)
{
    public ReleaseDiagnosticsSnapshot Capture()
    {
        var assembly = typeof(ReleaseDiagnosticsService).Assembly;
        var version = assembly
            .GetName()
            .Version?
            .ToString()
            ?? "unbekannt";

        var informational = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion
            ?? version;

        var connection =
            configuration.GetConnectionString("WerkPilot")
            ?? Environment.GetEnvironmentVariable(
                "ConnectionStrings__WerkPilot")
            ?? Environment.GetEnvironmentVariable(
                "WERKPILOT_CONNECTION_STRING");

        var initialPassword =
            Environment.GetEnvironmentVariable(
                "WERKPILOT_ADMIN_INITIAL_PASSWORD");

        var demoFlag =
            Environment.GetEnvironmentVariable(
                "WERKPILOT_SEED_DEMO_DATA");

        var demoEnabled =
            bool.TryParse(demoFlag, out var enabled)
            && enabled;

        var baseDirectory =
            AppContext.BaseDirectory;

        var logDirectory =
            Path.GetFullPath(
                Path.Combine(baseDirectory, "logs"));

        return new ReleaseDiagnosticsSnapshot(
            version,
            informational,
            RuntimeInformation.FrameworkDescription,
            RuntimeInformation.OSDescription,
            RuntimeInformation.ProcessArchitecture.ToString(),
            hostEnvironment.EnvironmentName,
            baseDirectory,
            logDirectory,
            !string.IsNullOrWhiteSpace(connection),
            !string.IsNullOrWhiteSpace(initialPassword),
            demoEnabled,
            DateTimeOffset.UtcNow);
    }
}
