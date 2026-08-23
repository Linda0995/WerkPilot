using Microsoft.EntityFrameworkCore;
using WerkPilot.Application.Calculation;
using WerkPilot.Domain.Calculation;

namespace WerkPilot.Infrastructure.Persistence;

public sealed class CalculationRepository(WerkPilotDbContext dbContext)
    : ICalculationRepository
{
    public Task<OfferCalculation?> GetByOfferIdAsync(
        Guid offerId,
        CancellationToken cancellationToken) =>
        dbContext.OfferCalculations
            .Include(x => x.Items)
            .SingleOrDefaultAsync(x => x.OfferId == offerId, cancellationToken);

    public Task AddAsync(
        OfferCalculation calculation,
        CancellationToken cancellationToken) =>
        dbContext.OfferCalculations.AddAsync(calculation, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
