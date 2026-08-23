using WerkPilot.Domain.Messaging;

namespace WerkPilot.Application.Messaging;

public interface IDocumentEmailDispatchRepository
{
    Task<IReadOnlyList<DocumentEmailDispatch>> GetAllAsync(
        CancellationToken cancellationToken);

    Task<IReadOnlyList<DocumentEmailDispatch>> GetDueRetriesAsync(
        DateTimeOffset nowUtc,
        int maximumCount,
        CancellationToken cancellationToken);

    Task<DocumentEmailDispatch?> GetAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task AddAsync(
        DocumentEmailDispatch dispatch,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
