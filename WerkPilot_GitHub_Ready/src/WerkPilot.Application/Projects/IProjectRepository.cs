using WerkPilot.Domain.Projects;

namespace WerkPilot.Application.Projects;

public interface IProjectRepository
{
    Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken cancellationToken);
    Task<Project?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<Project?> GetBySourceOfferIdAsync(Guid offerId, CancellationToken cancellationToken);
    Task<string> GetNextProjectNumberAsync(int year, CancellationToken cancellationToken);
    Task AddAsync(Project project, CancellationToken cancellationToken);
    Task<int> CountActiveAsync(CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
