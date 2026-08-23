using WerkPilot.Domain.Identity;

namespace WerkPilot.Application.Identity;

public sealed record AuthenticationResult(
    bool Succeeded,
    string Message,
    Guid? UserId = null,
    string? DisplayName = null,
    UserRole? Role = null,
    bool MustChangePassword = false);
