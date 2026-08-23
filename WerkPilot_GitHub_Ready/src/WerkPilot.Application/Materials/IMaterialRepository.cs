using WerkPilot.Domain.Materials;

namespace WerkPilot.Application.Materials;

public interface IMaterialRepository
{
    Task<IReadOnlyList<MaterialItem>> SearchAsync(
        string? searchText,
        bool includeInactive,
        CancellationToken cancellationToken);

    Task<MaterialItem?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<MaterialItem?> FindByArticleNumberAsync(string articleNumber, CancellationToken cancellationToken);
    Task AddAsync(MaterialItem item, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
