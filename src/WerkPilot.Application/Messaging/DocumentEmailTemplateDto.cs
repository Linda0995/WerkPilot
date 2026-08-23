using WerkPilot.Domain.Messaging;

namespace WerkPilot.Application.Messaging;

public sealed record DocumentEmailTemplateDto(
    Guid Id,
    DocumentEmailType DocumentType,
    string Name,
    string SubjectTemplate,
    string BodyTemplate,
    bool IsDefault);
