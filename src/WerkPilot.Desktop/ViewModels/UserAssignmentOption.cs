namespace WerkPilot.Desktop.ViewModels;

public sealed record UserAssignmentOption(
    Guid? UserId,
    string DisplayName)
{
    public static UserAssignmentOption Unassigned { get; } =
        new(null, "Nicht zugewiesen");
}
