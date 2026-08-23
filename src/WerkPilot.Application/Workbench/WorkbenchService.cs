using WerkPilot.Application.Identity;
using WerkPilot.Application.Search;
using WerkPilot.Domain.Workbench;

namespace WerkPilot.Application.Workbench;

public sealed class WorkbenchService(
    IWorkbenchRepository repository,
    SessionContext session)
{
    public async Task RecordOpenAsync(
        GlobalSearchResult result,
        CancellationToken cancellationToken = default)
    {
        var userId = session.UserId
            ?? throw new InvalidOperationException("Keine aktive Benutzersitzung.");

        var type = result.Type.ToString();
        var item = await repository.FindAsync(userId, type, result.EntityId, cancellationToken);

        if (item is null)
        {
            item = new WorkbenchItem(
                userId,
                type,
                result.EntityId,
                result.SearchNumber,
                result.PrimaryText,
                result.SecondaryText);
            await repository.AddAsync(item, cancellationToken);
        }
        else
        {
            item.Touch(result.SearchNumber, result.PrimaryText, result.SecondaryText);
        }

        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WorkbenchItemDto>> GetRecentAsync(
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = session.UserId
            ?? throw new InvalidOperationException("Keine aktive Benutzersitzung.");

        return (await repository.GetAsync(userId, cancellationToken))
            .OrderByDescending(x => x.LastOpenedAtUtc)
            .Take(Math.Clamp(limit, 1, 100))
            .Select(Map)
            .ToArray();
    }

    public async Task<IReadOnlyList<WorkbenchItemDto>> GetFavoritesAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = session.UserId
            ?? throw new InvalidOperationException("Keine aktive Benutzersitzung.");

        return (await repository.GetAsync(userId, cancellationToken))
            .Where(x => x.IsFavorite)
            .OrderBy(x => x.ItemType)
            .ThenBy(x => x.Title)
            .Select(Map)
            .ToArray();
    }

    public async Task ToggleFavoriteAsync(
        Guid workbenchItemId,
        CancellationToken cancellationToken = default)
    {
        var item = await repository.GetByIdAsync(workbenchItemId, cancellationToken)
            ?? throw new InvalidOperationException("Schnellzugriff wurde nicht gefunden.");

        if (item.UserId != session.UserId)
            throw new UnauthorizedAccessException("Der Schnellzugriff gehört zu einem anderen Benutzer.");

        item.SetFavorite(!item.IsFavorite);
        await repository.SaveChangesAsync(cancellationToken);
    }

    private static WorkbenchItemDto Map(WorkbenchItem x) => new(
        x.Id, x.ItemType, x.EntityId, x.Number, x.Title,
        x.Subtitle, x.IsFavorite, x.LastOpenedAtUtc);
}
