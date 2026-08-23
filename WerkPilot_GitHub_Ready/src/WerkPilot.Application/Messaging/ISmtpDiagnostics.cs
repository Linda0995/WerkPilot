namespace WerkPilot.Application.Messaging;

public interface ISmtpDiagnostics
{
    Task<SmtpDiagnosticResult> TestAsync(
        CancellationToken cancellationToken = default);
}
