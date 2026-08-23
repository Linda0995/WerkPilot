namespace WerkPilot.Application.Work;

public sealed record MyWorkItemDto(
    MyWorkItemType Type,
    Guid SourceId,
    Guid? CustomerId,
    Guid? ProjectId,
    string Reference,
    string Context,
    string Title,
    string? AssignedTo,
    DateTimeOffset? DueAtUtc,
    string Priority,
    string Status,
    bool IsDueToday,
    bool IsOverdue)
{
    public string TypeText => Type switch
    {
        MyWorkItemType.CustomerFollowUp => "Kunden-Aufgabe",
        MyWorkItemType.ProjectTask => "Projekt-Aufgabe",
        _ => Type.ToString()
    };
}
