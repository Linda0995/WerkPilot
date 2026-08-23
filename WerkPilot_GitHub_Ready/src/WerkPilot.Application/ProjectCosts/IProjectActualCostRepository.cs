using WerkPilot.Domain.ProjectCosts;

namespace WerkPilot.Application.ProjectCosts;

public interface IProjectActualCostRepository
{
    Task<IReadOnlyList<ProjectActualCost>> GetForProjectAsync(
        Guid projectId,
        CancellationToken cancellationToken);

    Task<ProjectActualCost?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(ProjectActualCost cost, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
