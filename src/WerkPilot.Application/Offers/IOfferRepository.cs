using WerkPilot.Domain.Offers;

namespace WerkPilot.Application.Offers;

public interface IOfferRepository
{
    Task<IReadOnlyList<Offer>> GetAllAsync(CancellationToken cancellationToken);
    Task<Offer?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<string> GetNextOfferNumberAsync(int year, CancellationToken cancellationToken);
    Task AddAsync(Offer offer, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
