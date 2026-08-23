using System.Net;
using System.Net.Mail;
using System.Text;
using WerkPilot.Application.Messaging;

namespace WerkPilot.Infrastructure.Messaging;

public sealed class SmtpEmailSender : IEmailSender
{
    public async Task SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default)
    {
        var settings = SmtpSettings.FromEnvironment();

        using var mail = new MailMessage
        {
            From = new MailAddress(settings.FromAddress, settings.FromDisplayName),
            Subject = message.Subject,
            Body = message.Body,
            IsBodyHtml = false,
            BodyEncoding = Encoding.UTF8,
            SubjectEncoding = Encoding.UTF8
        };

        mail.To.Add(new MailAddress(message.Recipient));

        foreach (var attachment in message.Attachments)
        {
            var stream = new MemoryStream(attachment.Content, writable: false);
            mail.Attachments.Add(new Attachment(stream, attachment.FileName, attachment.ContentType));
        }

        using var client = new SmtpClient(settings.Host, settings.Port)
        {
            EnableSsl = settings.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(settings.UserName, settings.Password)
        };

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            await client.SendMailAsync(mail, cancellationToken);
        }
        catch (Exception exception) when (
            exception is SmtpException or InvalidOperationException)
        {
            throw new EmailTransportException(
                "Der SMTP-Versand ist fehlgeschlagen. Bitte SMTP-Konfiguration und Netzwerk prüfen.",
                exception);
        }
    }

    private sealed record SmtpSettings(
        string Host,
        int Port,
        bool EnableSsl,
        string UserName,
        string Password,
        string FromAddress,
        string FromDisplayName)
    {
        public static SmtpSettings FromEnvironment()
        {
            var host = Require("WERKPILOT_SMTP_HOST");
            var userName = Require("WERKPILOT_SMTP_USERNAME");
            var password = Require("WERKPILOT_SMTP_PASSWORD");
            var fromAddress = Environment.GetEnvironmentVariable("WERKPILOT_SMTP_FROM")
                ?? userName;
            var displayName = Environment.GetEnvironmentVariable("WERKPILOT_SMTP_FROM_NAME")
                ?? "WerkPilot";
            var portText = Environment.GetEnvironmentVariable("WERKPILOT_SMTP_PORT") ?? "587";
            var sslText = Environment.GetEnvironmentVariable("WERKPILOT_SMTP_SSL") ?? "true";

            if (!int.TryParse(portText, out var port) || port is < 1 or > 65535)
                throw new EmailTransportException("WERKPILOT_SMTP_PORT ist ungültig.");

            if (!bool.TryParse(sslText, out var enableSsl))
                throw new EmailTransportException("WERKPILOT_SMTP_SSL muss true oder false sein.");

            return new SmtpSettings(
                host,
                port,
                enableSsl,
                userName,
                password,
                fromAddress,
                displayName);
        }

        private static string Require(string name) =>
            Environment.GetEnvironmentVariable(name)
            ?? throw new EmailTransportException(
                $"Die erforderliche Umgebungsvariable {name} ist nicht gesetzt.");
    }
}
