using Microsoft.EntityFrameworkCore;
using WerkPilot.Application.Purchasing;
using WerkPilot.Domain.Purchasing;

namespace WerkPilot.Infrastructure.Persistence;

public sealed class PurchaseListRepository(WerkPilotDbContext dbContext)
    : IPurchaseListRepository
{
    public async Task<IReadOnlyList<PurchaseList>> GetAllAsync(
        CancellationToken cancellationToken) =>
        await dbContext.PurchaseLists
            .Include(x => x.Items)
            .OrderByDescending(x => x.CreatedAtUtc)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public Task<PurchaseList?> GetAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        dbContext.PurchaseLists
            .Include(x => x.Items)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<PurchaseList?> GetByOfferIdAsync(
        Guid offerId,
        CancellationToken cancellationToken) =>
        dbContext.PurchaseLists
            .Include(x => x.Items)
            .SingleOrDefaultAsync(x => x.OfferId == offerId, cancellationToken);

    public async Task<string> GetNextNumberAsync(
        int year,
        CancellationToken cancellationToken)
    {
        var prefix = $"BL-{year}-";
        var values = await dbContext.PurchaseLists
            .IgnoreQueryFilters()
            .Where(x => x.PurchaseListNumber.StartsWith(prefix))
            .Select(x => x.PurchaseListNumber)
            .ToListAsync(cancellationToken);

        var maximum = values
            .Select(x => int.TryParse(x[prefix.Length..], out var number) ? number : 0)
            .DefaultIfEmpty()
            .Max();

        return $"{prefix}{maximum + 1:0000}";
    }

    public Task AddAsync(
        PurchaseList purchaseList,
        CancellationToken cancellationToken) =>
        dbContext.PurchaseLists.AddAsync(purchaseList, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
