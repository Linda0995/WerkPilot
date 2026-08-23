using System.Net.Sockets;
using WerkPilot.Application.Messaging;

namespace WerkPilot.Infrastructure.Messaging;

public sealed class SmtpDiagnostics : ISmtpDiagnostics
{
    public async Task<SmtpDiagnosticResult> TestAsync(
        CancellationToken cancellationToken = default)
    {
        var host = Environment.GetEnvironmentVariable("WERKPILOT_SMTP_HOST");
        var userName = Environment.GetEnvironmentVariable("WERKPILOT_SMTP_USERNAME");
        var password = Environment.GetEnvironmentVariable("WERKPILOT_SMTP_PASSWORD");
        var fromAddress = Environment.GetEnvironmentVariable("WERKPILOT_SMTP_FROM")
            ?? userName
            ?? string.Empty;
        var portText = Environment.GetEnvironmentVariable("WERKPILOT_SMTP_PORT")
            ?? "587";
        var sslText = Environment.GetEnvironmentVariable("WERKPILOT_SMTP_SSL")
            ?? "true";

        if (string.IsNullOrWhiteSpace(host) ||
            string.IsNullOrWhiteSpace(userName) ||
            string.IsNullOrWhiteSpace(password))
        {
            return new SmtpDiagnosticResult(
                false,
                false,
                host ?? string.Empty,
                ParsePort(portText),
                ParseBoolean(sslText),
                fromAddress,
                "SMTP ist nicht vollständig konfiguriert. Host, Benutzername und Passwort müssen gesetzt sein.");
        }

        var port = ParsePort(portText);
        var enableSsl = ParseBoolean(sslText);

        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(host, port, cancellationToken);

            return new SmtpDiagnosticResult(
                true,
                true,
                host,
                port,
                enableSsl,
                fromAddress,
                "SMTP-Konfiguration ist vorhanden und der Server ist über das Netzwerk erreichbar. Zugangsdaten werden erst bei einem echten Versand authentifiziert.");
        }
        catch (Exception exception) when (
            exception is SocketException or IOException or OperationCanceledException)
        {
            if (exception is OperationCanceledException)
                throw;

            return new SmtpDiagnosticResult(
                true,
                false,
                host,
                port,
                enableSsl,
                fromAddress,
                $"SMTP ist konfiguriert, der Server konnte aber nicht erreicht werden: {exception.Message}");
        }
    }

    private static int ParsePort(string value) =>
        int.TryParse(value, out var port) && port is >= 1 and <= 65535
            ? port
            : 587;

    private static bool ParseBoolean(string value) =>
        bool.TryParse(value, out var parsed) ? parsed : true;
}
