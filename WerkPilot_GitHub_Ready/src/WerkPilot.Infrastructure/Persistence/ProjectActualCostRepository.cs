using Microsoft.EntityFrameworkCore;
using WerkPilot.Application.ProjectCosts;
using WerkPilot.Domain.ProjectCosts;

namespace WerkPilot.Infrastructure.Persistence;

public sealed class ProjectActualCostRepository(WerkPilotDbContext dbContext)
    : IProjectActualCostRepository
{
    public async Task<IReadOnlyList<ProjectActualCost>> GetForProjectAsync(
        Guid projectId,
        CancellationToken cancellationToken) =>
        await dbContext.ProjectActualCosts
            .Where(x => x.ProjectId == projectId)
            .OrderByDescending(x => x.CostDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public Task<ProjectActualCost?> GetAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        dbContext.ProjectActualCosts.SingleOrDefaultAsync(
            x => x.Id == id, cancellationToken);

    public Task AddAsync(
        ProjectActualCost cost,
        CancellationToken cancellationToken) =>
        dbContext.ProjectActualCosts.AddAsync(cost, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
