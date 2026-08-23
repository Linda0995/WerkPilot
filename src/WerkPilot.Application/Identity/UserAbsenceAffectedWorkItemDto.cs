namespace WerkPilot.Application.Identity;

public enum UserAbsenceAffectedWorkType
{
    CustomerFollowUp = 1,
    ProjectTask = 2
}

public sealed record UserAbsenceAffectedWorkItemDto(
    UserAbsenceAffectedWorkType Type,
    Guid SourceId,
    Guid? ProjectId,
    string Reference,
    string Context,
    string Title,
    string Priority,
    DateOnly? DueDate,
    bool DueDuringAbsence)
{
    public string TypeText => Type switch
    {
        UserAbsenceAffectedWorkType.CustomerFollowUp => "Kunden-Aufgabe",
        UserAbsenceAffectedWorkType.ProjectTask => "Projekt-Aufgabe",
        _ => Type.ToString()
    };
}
