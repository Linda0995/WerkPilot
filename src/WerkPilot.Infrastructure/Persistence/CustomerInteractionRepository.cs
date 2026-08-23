using Microsoft.EntityFrameworkCore;
using WerkPilot.Application.Crm;
using WerkPilot.Domain.Crm;

namespace WerkPilot.Infrastructure.Persistence;

public sealed class CustomerInteractionRepository(WerkPilotDbContext dbContext)
    : ICustomerInteractionRepository
{
    public async Task<IReadOnlyList<CustomerInteraction>> GetForCustomerAsync(
        Guid customerId,
        CancellationToken cancellationToken) =>
        await dbContext.CustomerInteractions
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.OccurredAtUtc)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CustomerInteraction>> GetOpenFollowUpsAsync(
        DateOnly dueUntil,
        CancellationToken cancellationToken) =>
        await dbContext.CustomerInteractions
            .Where(x =>
                x.FollowUpDate.HasValue &&
                x.FollowUpDate.Value <= dueUntil &&
                !x.FollowUpCompleted)
            .OrderBy(x => x.FollowUpDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public Task<CustomerInteraction?> GetAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        dbContext.CustomerInteractions
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task AddAsync(
        CustomerInteraction interaction,
        CancellationToken cancellationToken) =>
        dbContext.CustomerInteractions.AddAsync(interaction, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
