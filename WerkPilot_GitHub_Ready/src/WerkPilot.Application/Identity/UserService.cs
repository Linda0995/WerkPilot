using WerkPilot.Application.Auditing;
using WerkPilot.Domain.Identity;

namespace WerkPilot.Application.Identity;

public sealed class UserService(
    IUserRepository repository,
    IAuditTrail auditTrail,
    IPasswordHasher passwordHasher)
{
    public async Task<IReadOnlyList<UserDto>> GetAllAsync(
        CancellationToken cancellationToken = default) =>
        (await repository.GetAllAsync(cancellationToken))
            .Select(Map)
            .ToArray();

    public async Task<UserDto> CreateAsync(
        CreateUserRequest request,
        string initialPassword,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.UserName))
            throw new UserValidationException("Der Benutzername ist erforderlich.");

        if (string.IsNullOrWhiteSpace(request.DisplayName))
            throw new UserValidationException("Der Anzeigename ist erforderlich.");

        PasswordPolicy.Validate(initialPassword, initialPassword);

        var normalized = request.UserName.Trim().ToLowerInvariant();

        if (await repository.FindByUserNameAsync(
                normalized,
                cancellationToken) is not null)
        {
            throw new UserValidationException(
                "Dieser Benutzername ist bereits vergeben.");
        }

        var user = new AppUser(
            normalized,
            request.DisplayName,
            request.Role);

        var password = passwordHasher.Hash(initialPassword);
        user.SetPassword(
            password.Hash,
            password.Salt,
            mustChangePassword: true);

        await repository.AddAsync(user, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        await auditTrail.WriteAsync(
            "User",
            user.Id,
            "Created",
            $"Benutzer {user.DisplayName} wurde angelegt.",
            cancellationToken);

        return Map(user);
    }

    public async Task SetActiveAsync(
        Guid id,
        bool active,
        CancellationToken cancellationToken = default)
    {
        var user = await repository.GetAsync(id, cancellationToken)
            ?? throw new InvalidOperationException(
                "Benutzer wurde nicht gefunden.");

        if (active)
            user.Activate();
        else
            user.Deactivate();

        await repository.SaveChangesAsync(cancellationToken);

        await auditTrail.WriteAsync(
            "User",
            user.Id,
            active ? "Activated" : "Deactivated",
            active
                ? "Benutzer wurde aktiviert."
                : "Benutzer wurde deaktiviert.",
            cancellationToken);
    }

    private static UserDto Map(AppUser user) => new(
        user.Id,
        user.UserName,
        user.DisplayName,
        user.Role,
        user.IsActive);
}
