using WerkPilot.Domain.Messaging;

namespace WerkPilot.Application.Messaging;

public sealed record SendDocumentEmailRequest(
    DocumentEmailType DocumentType,
    Guid DocumentId,
    string Recipient,
    string? SubjectOverride,
    string? BodyOverride);
