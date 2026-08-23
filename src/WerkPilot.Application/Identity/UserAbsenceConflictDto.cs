namespace WerkPilot.Application.Identity;

public sealed record UserAbsenceConflictDto(
    Guid UserId,
    string UserDisplayName,
    int OpenCustomerFollowUps,
    int OpenProjectTasks,
    int DueDuringAbsence,
    bool HasSubstitute)
{
    public int OpenWorkCount => OpenCustomerFollowUps + OpenProjectTasks;
    public bool RequiresAction => OpenWorkCount > 0 && !HasSubstitute;
}
