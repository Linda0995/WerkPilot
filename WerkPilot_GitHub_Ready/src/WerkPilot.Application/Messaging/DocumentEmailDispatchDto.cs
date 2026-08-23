using WerkPilot.Domain.Messaging;

namespace WerkPilot.Application.Messaging;

public sealed record DocumentEmailDispatchDto(
    Guid Id,
    DocumentEmailType DocumentType,
    Guid DocumentId,
    string DocumentNumber,
    string Recipient,
    string Subject,
    string AttachmentFileName,
    string? CreatedBy,
    DocumentEmailStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? SentAtUtc,
    string? ErrorMessage,
    int AttemptCount,
    DateTimeOffset? LastAttemptAtUtc,
    DateTimeOffset? NextRetryAtUtc);
