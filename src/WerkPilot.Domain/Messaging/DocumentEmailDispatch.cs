using WerkPilot.Domain.Common;

namespace WerkPilot.Domain.Messaging;

public sealed class DocumentEmailDispatch : Entity
{
    private DocumentEmailDispatch() { }

    public DocumentEmailDispatch(
        DocumentEmailType documentType,
        Guid documentId,
        string documentNumber,
        string recipient,
        string subject,
        string body,
        string attachmentFileName,
        string? createdBy)
    {
        if (documentId == Guid.Empty)
            throw new ArgumentException("Beleg erforderlich.", nameof(documentId));
        if (string.IsNullOrWhiteSpace(documentNumber))
            throw new ArgumentException("Belegnummer erforderlich.", nameof(documentNumber));
        if (string.IsNullOrWhiteSpace(recipient))
            throw new ArgumentException("Empfänger erforderlich.", nameof(recipient));
        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Betreff erforderlich.", nameof(subject));
        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Nachricht erforderlich.", nameof(body));
        if (string.IsNullOrWhiteSpace(attachmentFileName))
            throw new ArgumentException("Anhang erforderlich.", nameof(attachmentFileName));

        DocumentType = documentType;
        DocumentId = documentId;
        DocumentNumber = documentNumber.Trim();
        Recipient = recipient.Trim();
        Subject = subject.Trim();
        Body = body.Trim();
        AttachmentFileName = attachmentFileName.Trim();
        CreatedBy = Clean(createdBy);
        Status = DocumentEmailStatus.Prepared;
    }

    public DocumentEmailType DocumentType { get; private set; }
    public Guid DocumentId { get; private set; }
    public string DocumentNumber { get; private set; } = string.Empty;
    public string Recipient { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public string AttachmentFileName { get; private set; } = string.Empty;
    public string? CreatedBy { get; private set; }
    public DocumentEmailStatus Status { get; private set; }
    public DateTimeOffset? SentAtUtc { get; private set; }
    public string? ErrorMessage { get; private set; }
    public int AttemptCount { get; private set; }
    public DateTimeOffset? LastAttemptAtUtc { get; private set; }
    public DateTimeOffset? NextRetryAtUtc { get; private set; }

    public void BeginAttempt()
    {
        if (Status == DocumentEmailStatus.Sent)
            throw new InvalidOperationException("Erfolgreich versendete E-Mails werden nicht erneut versendet.");

        AttemptCount++;
        LastAttemptAtUtc = DateTimeOffset.UtcNow;
        NextRetryAtUtc = null;
        Status = DocumentEmailStatus.Prepared;
        ErrorMessage = null;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void MarkSent()
    {
        if (Status == DocumentEmailStatus.Sent)
            throw new InvalidOperationException("Der Versand wurde bereits als erfolgreich protokolliert.");

        Status = DocumentEmailStatus.Sent;
        SentAtUtc = DateTimeOffset.UtcNow;
        NextRetryAtUtc = null;
        ErrorMessage = null;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void MarkFailed(string errorMessage, DateTimeOffset? nextRetryAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Fehlertext erforderlich.", nameof(errorMessage));

        Status = DocumentEmailStatus.Failed;
        ErrorMessage = errorMessage.Trim();
        NextRetryAtUtc = nextRetryAtUtc;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void ScheduleRetry(DateTimeOffset retryAtUtc)
    {
        if (Status != DocumentEmailStatus.Failed)
            throw new InvalidOperationException("Nur fehlgeschlagene Sendungen können eingeplant werden.");
        if (retryAtUtc <= DateTimeOffset.UtcNow)
            throw new ArgumentException("Der Wiederholungszeitpunkt muss in der Zukunft liegen.");

        NextRetryAtUtc = retryAtUtc;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
