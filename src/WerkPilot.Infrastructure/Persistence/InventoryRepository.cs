using Microsoft.EntityFrameworkCore;
using WerkPilot.Application.Inventory;
using WerkPilot.Domain.Inventory;

namespace WerkPilot.Infrastructure.Persistence;

public sealed class InventoryRepository(WerkPilotDbContext dbContext)
    : IInventoryRepository
{
    public async Task<IReadOnlyList<InventoryItem>> GetAllAsync(
        CancellationToken cancellationToken) =>
        await dbContext.InventoryItems
            .OrderBy(x => x.MaterialItemId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public Task<InventoryItem?> GetAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        dbContext.InventoryItems.SingleOrDefaultAsync(
            x => x.Id == id,
            cancellationToken);

    public Task<InventoryItem?> GetByMaterialIdAsync(
        Guid materialItemId,
        CancellationToken cancellationToken) =>
        dbContext.InventoryItems.SingleOrDefaultAsync(
            x => x.MaterialItemId == materialItemId,
            cancellationToken);

    public async Task<IReadOnlyList<InventoryMovement>> GetMovementsAsync(
        Guid inventoryItemId,
        CancellationToken cancellationToken) =>
        await dbContext.InventoryMovements
            .Where(x => x.InventoryItemId == inventoryItemId)
            .OrderByDescending(x => x.OccurredAtUtc)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public Task AddItemAsync(
        InventoryItem item,
        CancellationToken cancellationToken) =>
        dbContext.InventoryItems.AddAsync(item, cancellationToken).AsTask();

    public Task AddMovementAsync(
        InventoryMovement movement,
        CancellationToken cancellationToken) =>
        dbContext.InventoryMovements.AddAsync(movement, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
