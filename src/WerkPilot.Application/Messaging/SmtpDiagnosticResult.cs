namespace WerkPilot.Application.Messaging;

public sealed record SmtpDiagnosticResult(
    bool IsConfigured,
    bool NetworkReachable,
    string Host,
    int Port,
    bool EnableSsl,
    string FromAddress,
    string Message);
