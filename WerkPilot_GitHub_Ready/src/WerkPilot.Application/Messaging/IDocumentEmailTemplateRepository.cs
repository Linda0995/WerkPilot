using WerkPilot.Domain.Messaging;

namespace WerkPilot.Application.Messaging;

public interface IDocumentEmailTemplateRepository
{
    Task<IReadOnlyList<DocumentEmailTemplate>> GetAllAsync(
        CancellationToken cancellationToken);

    Task<DocumentEmailTemplate?> GetAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task AddAsync(
        DocumentEmailTemplate template,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
