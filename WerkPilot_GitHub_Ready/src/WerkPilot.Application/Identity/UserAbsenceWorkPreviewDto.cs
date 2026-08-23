namespace WerkPilot.Application.Identity;

public sealed record UserAbsenceWorkPreviewDto(
    Guid AbsenceId,
    string UserDisplayName,
    string? SubstituteDisplayName,
    int TotalOpenCount,
    int DueDuringAbsenceCount,
    IReadOnlyList<UserAbsenceAffectedWorkItemDto> Items);
