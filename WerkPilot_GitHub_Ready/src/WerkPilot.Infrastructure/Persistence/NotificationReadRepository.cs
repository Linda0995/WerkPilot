using Microsoft.EntityFrameworkCore;
using WerkPilot.Application.Notifications;
using WerkPilot.Domain.Notifications;

namespace WerkPilot.Infrastructure.Persistence;

public sealed class NotificationReadRepository(WerkPilotDbContext dbContext) : INotificationReadRepository
{
    public async Task<IReadOnlySet<string>> GetReadKeysAsync(Guid userId, CancellationToken cancellationToken) =>
        (await dbContext.NotificationReadStates.AsNoTracking().Where(x => x.UserId == userId).Select(x => x.NotificationKey).ToListAsync(cancellationToken)).ToHashSet(StringComparer.Ordinal);
    public Task<NotificationReadState?> GetAsync(Guid userId, string key, CancellationToken cancellationToken) =>
        dbContext.NotificationReadStates.SingleOrDefaultAsync(x => x.UserId == userId && x.NotificationKey == key, cancellationToken);
    public Task AddAsync(NotificationReadState state, CancellationToken cancellationToken) => dbContext.NotificationReadStates.AddAsync(state, cancellationToken).AsTask();
    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}
