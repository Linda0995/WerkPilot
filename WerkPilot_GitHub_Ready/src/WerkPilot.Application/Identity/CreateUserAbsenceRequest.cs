using WerkPilot.Domain.Identity;

namespace WerkPilot.Application.Identity;

public sealed record CreateUserAbsenceRequest(
    Guid UserId,
    UserAbsenceType Type,
    DateOnly StartDate,
    DateOnly EndDate,
    Guid? SubstituteUserId,
    string? Note);
