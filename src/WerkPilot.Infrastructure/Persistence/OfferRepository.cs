using Microsoft.EntityFrameworkCore;
using WerkPilot.Application.Offers;
using WerkPilot.Domain.Offers;

namespace WerkPilot.Infrastructure.Persistence;

public sealed class OfferRepository(WerkPilotDbContext dbContext) : IOfferRepository
{
    public async Task<IReadOnlyList<Offer>> GetAllAsync(CancellationToken cancellationToken) =>
        await dbContext.Offers.Include(x => x.Positions)
            .OrderByDescending(x => x.OfferDate).ThenByDescending(x => x.OfferNumber)
            .ToListAsync(cancellationToken);

    public Task<Offer?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Offers.Include(x => x.Positions)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<string> GetNextOfferNumberAsync(int year, CancellationToken cancellationToken)
    {
        var prefix = $"AN-{year}-";
        var values = await dbContext.Offers.IgnoreQueryFilters()
            .Where(x => x.OfferNumber.StartsWith(prefix))
            .Select(x => x.OfferNumber).ToListAsync(cancellationToken);

        var maximum = values.Select(x => int.TryParse(x[prefix.Length..], out var n) ? n : 0)
            .DefaultIfEmpty().Max();

        return $"{prefix}{maximum + 1:0000}";
    }

    public Task AddAsync(Offer offer, CancellationToken cancellationToken) =>
        dbContext.Offers.AddAsync(offer, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
