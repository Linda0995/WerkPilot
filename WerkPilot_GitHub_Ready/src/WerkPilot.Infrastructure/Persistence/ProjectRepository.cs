using Microsoft.EntityFrameworkCore;
using WerkPilot.Application.Projects;
using WerkPilot.Domain.Projects;

namespace WerkPilot.Infrastructure.Persistence;

public sealed class ProjectRepository(WerkPilotDbContext dbContext)
    : IProjectRepository
{
    public async Task<IReadOnlyList<Project>> GetAllAsync(
        CancellationToken cancellationToken) =>
        await dbContext.Projects
            .Include(x => x.Tasks)
            .OrderByDescending(x => x.CreatedAtUtc)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public Task<Project?> GetAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        dbContext.Projects
            .Include(x => x.Tasks)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Project?> GetBySourceOfferIdAsync(
        Guid offerId,
        CancellationToken cancellationToken) =>
        dbContext.Projects
            .Include(x => x.Tasks)
            .SingleOrDefaultAsync(x => x.SourceOfferId == offerId, cancellationToken);

    public async Task<string> GetNextProjectNumberAsync(
        int year,
        CancellationToken cancellationToken)
    {
        var prefix = $"PR-{year}-";

        var numbers = await dbContext.Projects
            .IgnoreQueryFilters()
            .Where(x => x.ProjectNumber.StartsWith(prefix))
            .Select(x => x.ProjectNumber)
            .ToListAsync(cancellationToken);

        var maximum = numbers
            .Select(x => int.TryParse(x[prefix.Length..], out var number) ? number : 0)
            .DefaultIfEmpty()
            .Max();

        return $"{prefix}{maximum + 1:0000}";
    }

    public Task AddAsync(
        Project project,
        CancellationToken cancellationToken) =>
        dbContext.Projects.AddAsync(project, cancellationToken).AsTask();

    public Task<int> CountActiveAsync(CancellationToken cancellationToken) =>
        dbContext.Projects.CountAsync(
            x => x.Status == ProjectStatus.Planned ||
                 x.Status == ProjectStatus.Active ||
                 x.Status == ProjectStatus.OnHold,
            cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
