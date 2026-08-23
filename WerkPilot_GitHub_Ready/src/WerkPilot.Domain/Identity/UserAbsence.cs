using WerkPilot.Domain.Common;

namespace WerkPilot.Domain.Identity;

public sealed class UserAbsence : Entity
{
    private UserAbsence() { }

    public UserAbsence(
        Guid userId,
        string userDisplayName,
        UserAbsenceType type,
        DateOnly startDate,
        DateOnly endDate,
        Guid? substituteUserId,
        string? substituteDisplayName,
        string? note,
        string? createdBy)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Benutzer erforderlich.", nameof(userId));
        if (string.IsNullOrWhiteSpace(userDisplayName))
            throw new ArgumentException("Benutzername erforderlich.", nameof(userDisplayName));
        if (endDate < startDate)
            throw new ArgumentException("Enddatum darf nicht vor dem Startdatum liegen.");
        if (substituteUserId == userId)
            throw new ArgumentException("Ein Benutzer kann nicht seine eigene Vertretung sein.");

        UserId = userId;
        UserDisplayName = userDisplayName.Trim();
        Type = type;
        StartDate = startDate;
        EndDate = endDate;
        SubstituteUserId = substituteUserId;
        SubstituteDisplayName = Clean(substituteDisplayName);
        Note = Clean(note);
        CreatedBy = Clean(createdBy);
        Status = UserAbsenceStatus.Planned;
    }

    public Guid UserId { get; private set; }
    public string UserDisplayName { get; private set; } = string.Empty;
    public UserAbsenceType Type { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public Guid? SubstituteUserId { get; private set; }
    public string? SubstituteDisplayName { get; private set; }
    public string? Note { get; private set; }
    public string? CreatedBy { get; private set; }
    public UserAbsenceStatus Status { get; private set; }

    public bool Includes(DateOnly date) =>
        Status != UserAbsenceStatus.Cancelled
        && date >= StartDate
        && date <= EndDate;

    public bool Overlaps(DateOnly startDate, DateOnly endDate) =>
        Status != UserAbsenceStatus.Cancelled
        && startDate <= EndDate
        && endDate >= StartDate;

    public void RefreshStatus(DateOnly today)
    {
        if (Status == UserAbsenceStatus.Cancelled)
            return;

        Status = today switch
        {
            _ when today < StartDate => UserAbsenceStatus.Planned,
            _ when today > EndDate => UserAbsenceStatus.Completed,
            _ => UserAbsenceStatus.Active
        };

        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Cancel()
    {
        Status = UserAbsenceStatus.Cancelled;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
