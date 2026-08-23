using WerkPilot.Domain.Crm;

namespace WerkPilot.Application.Crm;

public interface ICustomerInteractionRepository
{
    Task<IReadOnlyList<CustomerInteraction>> GetForCustomerAsync(
        Guid customerId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<CustomerInteraction>> GetOpenFollowUpsAsync(
        DateOnly dueUntil,
        CancellationToken cancellationToken);

    Task<CustomerInteraction?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(CustomerInteraction interaction, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
