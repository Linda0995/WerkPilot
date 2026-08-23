using Microsoft.EntityFrameworkCore;
using WerkPilot.Application.Messaging;
using WerkPilot.Domain.Messaging;

namespace WerkPilot.Infrastructure.Persistence;

public sealed class DocumentEmailDispatchRepository(WerkPilotDbContext dbContext)
    : IDocumentEmailDispatchRepository
{
    public async Task<IReadOnlyList<DocumentEmailDispatch>> GetAllAsync(
        CancellationToken cancellationToken) =>
        await dbContext.DocumentEmailDispatches
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<DocumentEmailDispatch>> GetDueRetriesAsync(
        DateTimeOffset nowUtc,
        int maximumCount,
        CancellationToken cancellationToken) =>
        await dbContext.DocumentEmailDispatches
            .Where(x =>
                x.Status == DocumentEmailStatus.Failed &&
                x.NextRetryAtUtc.HasValue &&
                x.NextRetryAtUtc.Value <= nowUtc)
            .OrderBy(x => x.NextRetryAtUtc)
            .Take(maximumCount)
            .ToListAsync(cancellationToken);

    public Task<DocumentEmailDispatch?> GetAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        dbContext.DocumentEmailDispatches
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task AddAsync(
        DocumentEmailDispatch dispatch,
        CancellationToken cancellationToken) =>
        dbContext.DocumentEmailDispatches
            .AddAsync(dispatch, cancellationToken)
            .AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
