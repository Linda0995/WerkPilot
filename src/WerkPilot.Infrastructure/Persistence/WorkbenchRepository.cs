using Microsoft.EntityFrameworkCore;
using WerkPilot.Application.Workbench;
using WerkPilot.Domain.Workbench;

namespace WerkPilot.Infrastructure.Persistence;

public sealed class WorkbenchRepository(WerkPilotDbContext dbContext)
    : IWorkbenchRepository
{
    public async Task<IReadOnlyList<WorkbenchItem>> GetAsync(
        Guid userId,
        CancellationToken cancellationToken) =>
        await dbContext.WorkbenchItems
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

    public Task<WorkbenchItem?> FindAsync(
        Guid userId,
        string itemType,
        Guid entityId,
        CancellationToken cancellationToken) =>
        dbContext.WorkbenchItems.SingleOrDefaultAsync(
            x => x.UserId == userId &&
                 x.ItemType == itemType &&
                 x.EntityId == entityId,
            cancellationToken);

    public Task<WorkbenchItem?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        dbContext.WorkbenchItems.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task AddAsync(
        WorkbenchItem item,
        CancellationToken cancellationToken) =>
        dbContext.WorkbenchItems.AddAsync(item, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
