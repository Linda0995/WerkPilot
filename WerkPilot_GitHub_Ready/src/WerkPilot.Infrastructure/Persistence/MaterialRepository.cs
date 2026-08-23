using Microsoft.EntityFrameworkCore;
using WerkPilot.Application.Materials;
using WerkPilot.Domain.Materials;

namespace WerkPilot.Infrastructure.Persistence;

public sealed class MaterialRepository(WerkPilotDbContext dbContext)
    : IMaterialRepository
{
    public async Task<IReadOnlyList<MaterialItem>> SearchAsync(
        string? searchText,
        bool includeInactive,
        CancellationToken cancellationToken)
    {
        IQueryable<MaterialItem> query = includeInactive
            ? dbContext.MaterialItems.IgnoreQueryFilters()
            : dbContext.MaterialItems;

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var term = searchText.Trim();
            query = query.Where(x =>
                EF.Functions.ILike(x.ArticleNumber, $"%{term}%") ||
                EF.Functions.ILike(x.Description, $"%{term}%") ||
                (x.Supplier != null && EF.Functions.ILike(x.Supplier, $"%{term}%")) ||
                (x.SupplierArticleNumber != null &&
                    EF.Functions.ILike(x.SupplierArticleNumber, $"%{term}%")));
        }

        return await query
            .OrderBy(x => x.ArticleNumber)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<MaterialItem?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.MaterialItems
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<MaterialItem?> FindByArticleNumberAsync(
        string articleNumber,
        CancellationToken cancellationToken)
    {
        var normalized = articleNumber.Trim().ToUpperInvariant();

        return dbContext.MaterialItems
            .IgnoreQueryFilters()
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.ArticleNumber.ToUpper() == normalized,
                cancellationToken);
    }

    public Task AddAsync(MaterialItem item, CancellationToken cancellationToken) =>
        dbContext.MaterialItems.AddAsync(item, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
