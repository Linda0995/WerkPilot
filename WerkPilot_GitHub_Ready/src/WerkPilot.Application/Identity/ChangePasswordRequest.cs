namespace WerkPilot.Application.Identity;

public sealed record ChangePasswordRequest(
    Guid UserId,
    string CurrentPassword,
    string NewPassword,
    string Confirmation);
