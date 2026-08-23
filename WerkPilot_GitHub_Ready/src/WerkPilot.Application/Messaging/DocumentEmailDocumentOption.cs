using WerkPilot.Domain.Messaging;

namespace WerkPilot.Application.Messaging;

public sealed record DocumentEmailDocumentOption(
    DocumentEmailType DocumentType,
    Guid DocumentId,
    string DocumentNumber,
    string CustomerName,
    DateOnly DocumentDate,
    string StatusText)
{
    public string DisplayText =>
        $"{DocumentNumber} · {CustomerName} · {DocumentDate:dd.MM.yyyy}";
}
