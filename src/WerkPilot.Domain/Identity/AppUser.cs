using WerkPilot.Domain.Common;

namespace WerkPilot.Domain.Identity;

public sealed class AppUser : Entity
{
    private AppUser() { }

    public AppUser(string userName, string displayName, UserRole role)
    {
        ChangeUserName(userName);
        ChangeDisplayName(displayName);
        Role = role;
        IsActive = true;
    }

    public string UserName { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public string? PasswordHash { get; private set; }
    public string? PasswordSalt { get; private set; }
    public int FailedLoginCount { get; private set; }
    public DateTimeOffset? LockedUntilUtc { get; private set; }
    public DateTimeOffset? LastLoginAtUtc { get; private set; }
    public bool MustChangePassword { get; private set; }

    public bool IsLocked(DateTimeOffset nowUtc) => LockedUntilUtc > nowUtc;

    public void SetPassword(string passwordHash, string passwordSalt, bool mustChangePassword)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Der Passwort-Hash ist erforderlich.", nameof(passwordHash));
        if (string.IsNullOrWhiteSpace(passwordSalt))
            throw new ArgumentException("Der Passwort-Salt ist erforderlich.", nameof(passwordSalt));

        PasswordHash = passwordHash;
        PasswordSalt = passwordSalt;
        MustChangePassword = mustChangePassword;
        FailedLoginCount = 0;
        LockedUntilUtc = null;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void RecordSuccessfulLogin(DateTimeOffset nowUtc)
    {
        LastLoginAtUtc = nowUtc;
        FailedLoginCount = 0;
        LockedUntilUtc = null;
        UpdatedAtUtc = nowUtc;
    }

    public void RecordFailedLogin(DateTimeOffset nowUtc, int maximumAttempts, TimeSpan lockDuration)
    {
        FailedLoginCount++;
        if (FailedLoginCount >= maximumAttempts)
        {
            LockedUntilUtc = nowUtc.Add(lockDuration);
            FailedLoginCount = 0;
        }
        UpdatedAtUtc = nowUtc;
    }

    public void ConfirmPasswordChanged()
    {
        MustChangePassword = false;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void ChangeUserName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Der Benutzername ist erforderlich.", nameof(value));
        UserName = value.Trim().ToLowerInvariant();
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void ChangeDisplayName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Der Anzeigename ist erforderlich.", nameof(value));
        DisplayName = value.Trim();
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void ChangeRole(UserRole role) { Role = role; UpdatedAtUtc = DateTimeOffset.UtcNow; }
    public void Activate() { IsActive = true; UpdatedAtUtc = DateTimeOffset.UtcNow; }
    public void Deactivate() { IsActive = false; UpdatedAtUtc = DateTimeOffset.UtcNow; }
}
