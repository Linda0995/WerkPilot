using WerkPilot.Domain.Common;

namespace WerkPilot.Domain.Workbench;

public sealed class WorkbenchItem : Entity
{
    private WorkbenchItem() { }

    public WorkbenchItem(
        Guid userId,
        string itemType,
        Guid entityId,
        string number,
        string title,
        string? subtitle)
    {
        if (userId == Guid.Empty) throw new ArgumentException("Benutzer erforderlich.", nameof(userId));
        if (entityId == Guid.Empty) throw new ArgumentException("Datensatz erforderlich.", nameof(entityId));
        if (string.IsNullOrWhiteSpace(itemType)) throw new ArgumentException("Typ erforderlich.", nameof(itemType));
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Titel erforderlich.", nameof(title));

        UserId = userId;
        ItemType = itemType.Trim();
        EntityId = entityId;
        Number = number?.Trim() ?? string.Empty;
        Title = title.Trim();
        Subtitle = string.IsNullOrWhiteSpace(subtitle) ? null : subtitle.Trim();
        LastOpenedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid UserId { get; private set; }
    public string ItemType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public string Number { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string? Subtitle { get; private set; }
    public bool IsFavorite { get; private set; }
    public DateTimeOffset LastOpenedAtUtc { get; private set; }

    public void Touch(string number, string title, string? subtitle)
    {
        Number = number?.Trim() ?? string.Empty;
        Title = title.Trim();
        Subtitle = string.IsNullOrWhiteSpace(subtitle) ? null : subtitle.Trim();
        LastOpenedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void SetFavorite(bool favorite)
    {
        IsFavorite = favorite;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
