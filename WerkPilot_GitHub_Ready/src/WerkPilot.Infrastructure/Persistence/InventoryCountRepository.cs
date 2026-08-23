using Microsoft.EntityFrameworkCore;
using WerkPilot.Application.Inventory;
using WerkPilot.Domain.Inventory;

namespace WerkPilot.Infrastructure.Persistence;

public sealed class InventoryCountRepository(WerkPilotDbContext dbContext)
    : IInventoryCountRepository
{
    public async Task<IReadOnlyList<InventoryCount>> GetAllAsync(
        CancellationToken cancellationToken) =>
        await dbContext.InventoryCounts
            .Include(x => x.Lines)
            .OrderByDescending(x => x.CountDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public Task<InventoryCount?> GetAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        dbContext.InventoryCounts
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<string> GetNextNumberAsync(
        int year,
        CancellationToken cancellationToken)
    {
        var prefix = $"INV-{year}-";

        var numbers = await dbContext.InventoryCounts
            .IgnoreQueryFilters()
            .Where(x => x.CountNumber.StartsWith(prefix))
            .Select(x => x.CountNumber)
            .ToListAsync(cancellationToken);

        var maximum = numbers
            .Select(x => int.TryParse(x[prefix.Length..], out var value) ? value : 0)
            .DefaultIfEmpty()
            .Max();

        return $"{prefix}{maximum + 1:0000}";
    }

    public Task AddAsync(
        InventoryCount count,
        CancellationToken cancellationToken) =>
        dbContext.InventoryCounts.AddAsync(count, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
