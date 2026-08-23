using WerkPilot.Domain.Notifications;
namespace WerkPilot.Application.Notifications;
public interface INotificationReadRepository
{
    Task<IReadOnlySet<string>> GetReadKeysAsync(Guid userId, CancellationToken cancellationToken);
    Task<NotificationReadState?> GetAsync(Guid userId, string key, CancellationToken cancellationToken);
    Task AddAsync(NotificationReadState state, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
