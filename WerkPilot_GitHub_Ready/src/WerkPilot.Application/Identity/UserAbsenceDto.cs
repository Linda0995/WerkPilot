using WerkPilot.Domain.Identity;

namespace WerkPilot.Application.Identity;

public sealed record UserAbsenceDto(
    Guid Id,
    Guid UserId,
    string UserDisplayName,
    UserAbsenceType Type,
    DateOnly StartDate,
    DateOnly EndDate,
    Guid? SubstituteUserId,
    string? SubstituteDisplayName,
    string? Note,
    UserAbsenceStatus Status,
    bool IsActiveToday,
    int DurationDays);
