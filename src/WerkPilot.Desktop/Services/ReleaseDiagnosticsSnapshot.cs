namespace WerkPilot.Desktop.Services;

public sealed record ReleaseDiagnosticsSnapshot(
    string ProductVersion,
    string InformationalVersion,
    string DotnetRuntime,
    string OperatingSystem,
    string ProcessArchitecture,
    string EnvironmentName,
    string BaseDirectory,
    string LogDirectory,
    bool DatabaseConnectionConfigured,
    bool InitialAdminBootstrapConfigured,
    bool DemoDataEnabled,
    DateTimeOffset CheckedAtUtc);
