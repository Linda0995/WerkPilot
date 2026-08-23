using WerkPilot.Domain.Identity;

namespace WerkPilot.Application.Identity;

public sealed class SessionContext
{
    public bool IsAuthenticated => UserId.HasValue;
    public Guid? UserId { get; private set; }
    public string? DisplayName { get; private set; }
    public UserRole? Role { get; private set; }

    public void SignIn(Guid userId, string displayName, UserRole role)
    {
        UserId = userId;
        DisplayName = displayName;
        Role = role;
    }

    public void SignOut()
    {
        UserId = null;
        DisplayName = null;
        Role = null;
    }
}
