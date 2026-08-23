using Microsoft.EntityFrameworkCore;
using WerkPilot.Application.Crm;
using WerkPilot.Domain.Crm;

namespace WerkPilot.Infrastructure.Persistence;

public sealed class CustomerFollowUpRepository(WerkPilotDbContext dbContext)
    : ICustomerFollowUpRepository
{
    public async Task<IReadOnlyList<CustomerFollowUp>> GetAllAsync(
        CancellationToken cancellationToken) =>
        await dbContext.CustomerFollowUps
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public Task<CustomerFollowUp?> GetAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        dbContext.CustomerFollowUps
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task AddAsync(
        CustomerFollowUp followUp,
        CancellationToken cancellationToken) =>
        dbContext.CustomerFollowUps
            .AddAsync(followUp, cancellationToken)
            .AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
