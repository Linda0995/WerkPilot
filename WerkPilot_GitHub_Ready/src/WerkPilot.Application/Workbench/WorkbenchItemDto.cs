namespace WerkPilot.Application.Workbench;

public sealed record WorkbenchItemDto(
    Guid Id,
    string ItemType,
    Guid EntityId,
    string Number,
    string Title,
    string? Subtitle,
    bool IsFavorite,
    DateTimeOffset LastOpenedAtUtc);
