namespace WerkPilot.Desktop.Services;

public sealed record FirstRunReadinessSnapshot(
    bool DatabaseConfigured,
    bool DatabaseReachable,
    bool AdminBootstrapConfigured,
    bool DemoModeEnabled,
    string EnvironmentName,
    string ProductVersion,
    string Status,
    DateTimeOffset CheckedAtUtc)
{
    public bool IsReady =>
        DatabaseConfigured
        && DatabaseReachable
        && AdminBootstrapConfigured;
}
