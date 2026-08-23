using WerkPilot.Domain.Common;

namespace WerkPilot.Domain.Notifications;

public sealed class NotificationReadState : Entity
{
    private NotificationReadState() { }

    public NotificationReadState(Guid userId, string notificationKey)
    {
        if (userId == Guid.Empty) throw new ArgumentException("Benutzer erforderlich.", nameof(userId));
        if (string.IsNullOrWhiteSpace(notificationKey)) throw new ArgumentException("Benachrichtigungsschlüssel erforderlich.", nameof(notificationKey));
        UserId = userId;
        NotificationKey = notificationKey.Trim();
        ReadAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid UserId { get; private set; }
    public string NotificationKey { get; private set; } = string.Empty;
    public DateTimeOffset ReadAtUtc { get; private set; }

    public void MarkRead() { ReadAtUtc = DateTimeOffset.UtcNow; UpdatedAtUtc = ReadAtUtc; }
}
