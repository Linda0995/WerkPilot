using WerkPilot.Domain.Messaging;

namespace WerkPilot.Application.Messaging;

public sealed record DocumentEmailPreview(
    DocumentEmailType DocumentType,
    Guid DocumentId,
    string DocumentNumber,
    string CustomerName,
    string Recipient,
    string Subject,
    string Body,
    string AttachmentFileName);
