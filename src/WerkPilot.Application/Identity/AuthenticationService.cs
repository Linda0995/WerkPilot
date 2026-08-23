using WerkPilot.Application.Auditing;

namespace WerkPilot.Application.Identity;

public sealed class AuthenticationService(
    IUserRepository repository,
    IPasswordHasher passwordHasher,
    IAuditTrail auditTrail,
    SessionContext session)
{
    private const int MaximumAttempts = 5;
    private static readonly TimeSpan LockDuration = TimeSpan.FromMinutes(15);

    public async Task<AuthenticationResult> LoginAsync(
        string userName,
        string password,
        CancellationToken cancellationToken = default)
    {
        var normalized = userName?.Trim().ToLowerInvariant() ?? string.Empty;
        var user = await repository.FindByUserNameAsync(normalized, cancellationToken);
        if (user is null || !user.IsActive)
            return new AuthenticationResult(false, "Benutzername oder Passwort ist falsch.");

        var now = DateTimeOffset.UtcNow;
        if (user.IsLocked(now))
            return new AuthenticationResult(false, $"Benutzer ist bis {user.LockedUntilUtc:dd.MM.yyyy HH:mm} UTC gesperrt.");

        var valid = user.PasswordHash is not null && user.PasswordSalt is not null &&
                    passwordHasher.Verify(password, user.PasswordHash, user.PasswordSalt);
        if (!valid)
        {
            user.RecordFailedLogin(now, MaximumAttempts, LockDuration);
            await repository.SaveChangesAsync(cancellationToken);
            await auditTrail.WriteAsync("User", user.Id, "LoginFailed", "Fehlgeschlagener Anmeldeversuch.", cancellationToken);
            return new AuthenticationResult(false, "Benutzername oder Passwort ist falsch.");
        }

        user.RecordSuccessfulLogin(now);
        await repository.SaveChangesAsync(cancellationToken);
        session.SignIn(user.Id, user.DisplayName, user.Role);
        await auditTrail.WriteAsync("User", user.Id, "LoginSucceeded", "Benutzer hat sich angemeldet.", cancellationToken);

        return new AuthenticationResult(true, "Anmeldung erfolgreich.", user.Id, user.DisplayName, user.Role, user.MustChangePassword);
    }


    public async Task ChangePasswordAsync(
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        PasswordPolicy.Validate(request.NewPassword, request.Confirmation);

        var user = await repository.GetAsync(request.UserId, cancellationToken)
            ?? throw new InvalidOperationException("Benutzer wurde nicht gefunden.");

        if (user.PasswordHash is null || user.PasswordSalt is null ||
            !passwordHasher.Verify(request.CurrentPassword, user.PasswordHash, user.PasswordSalt))
            throw new UserValidationException("Das bisherige Kennwort ist nicht korrekt.");

        var password = passwordHasher.Hash(request.NewPassword);
        user.SetPassword(password.Hash, password.Salt, false);

        await repository.SaveChangesAsync(cancellationToken);
        await auditTrail.WriteAsync(
            "User",
            user.Id,
            "PasswordChanged",
            "Kennwort wurde geändert.",
            cancellationToken);
    }

    public void Logout() => session.SignOut();

}
