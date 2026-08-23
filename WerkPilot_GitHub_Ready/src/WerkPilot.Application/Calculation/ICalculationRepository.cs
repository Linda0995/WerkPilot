using WerkPilot.Domain.Calculation;

namespace WerkPilot.Application.Calculation;

public interface ICalculationRepository
{
    Task<OfferCalculation?> GetByOfferIdAsync(
        Guid offerId,
        CancellationToken cancellationToken);

    Task AddAsync(
        OfferCalculation calculation,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
