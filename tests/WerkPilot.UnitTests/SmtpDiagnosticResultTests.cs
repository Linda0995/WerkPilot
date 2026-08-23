using WerkPilot.Application.Messaging;

namespace WerkPilot.UnitTests;

public sealed class SmtpDiagnosticResultTests
{
    [Fact]
    public void Result_SeparatesConfigurationFromNetworkReachability()
    {
        var result = new SmtpDiagnosticResult(
            true,
            false,
            "smtp.example.com",
            587,
            true,
            "office@example.com",
            "Server nicht erreichbar.");

        Assert.True(result.IsConfigured);
        Assert.False(result.NetworkReachable);
        Assert.Equal(587, result.Port);
    }
}
