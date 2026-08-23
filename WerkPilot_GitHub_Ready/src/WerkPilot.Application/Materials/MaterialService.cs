using WerkPilot.Application.Auditing;
using WerkPilot.Domain.Materials;

namespace WerkPilot.Application.Materials;

public sealed class MaterialService(
    IMaterialRepository repository,
    IMaterialCsvSerializer csvSerializer,
    IAuditTrail auditTrail)
{
    public async Task<IReadOnlyList<MaterialItemDto>> SearchAsync(
        string? searchText,
        bool includeInactive = false,
        CancellationToken cancellationToken = default) =>
        (await repository.SearchAsync(searchText, includeInactive, cancellationToken))
            .Select(Map)
            .ToArray();


    public async Task<MaterialImportResult> ImportCsvAsync(
        string csvContent,
        CancellationToken cancellationToken = default)
    {
        var rows = csvSerializer.Parse(csvContent);
        var errors = new List<string>();
        var created = 0;
        var updated = 0;
        var skipped = 0;

        foreach (var row in rows)
        {
            try
            {
                var existing = await repository.FindByArticleNumberAsync(
                    row.ArticleNumber,
                    cancellationToken);

                if (existing is null)
                {
                    var item = new MaterialItem(
                        row.ArticleNumber,
                        row.Description,
                        row.Unit,
                        row.PurchasePrice);

                    item.Update(
                        row.ArticleNumber,
                        row.Description,
                        row.Unit,
                        row.PurchasePrice,
                        row.Supplier,
                        row.SupplierArticleNumber);

                    await repository.AddAsync(item, cancellationToken);
                    created++;
                }
                else
                {
                    existing.Update(
                        row.ArticleNumber,
                        row.Description,
                        row.Unit,
                        row.PurchasePrice,
                        row.Supplier,
                        row.SupplierArticleNumber);
                    updated++;
                }
            }
            catch (Exception exception)
            {
                skipped++;
                errors.Add($"Zeile {row.RowNumber}: {exception.Message}");
            }
        }

        await repository.SaveChangesAsync(cancellationToken);

        await auditTrail.WriteAsync(
            "MaterialImport",
            Guid.NewGuid(),
            "CsvImported",
            $"CSV-Import: {created} neu, {updated} aktualisiert, {skipped} übersprungen.",
            cancellationToken);

        return new MaterialImportResult(created, updated, skipped, errors);
    }

    public async Task<string> ExportCsvAsync(
        bool includeInactive = true,
        CancellationToken cancellationToken = default)
    {
        var items = await SearchAsync(null, includeInactive, cancellationToken);
        return csvSerializer.Serialize(items);
    }

    public async Task<MaterialItemDto> CreateAsync(
        string articleNumber,
        string description,
        string unit,
        decimal purchasePrice,
        string? supplier,
        string? supplierArticleNumber,
        CancellationToken cancellationToken = default)
    {
        var existing = await repository.FindByArticleNumberAsync(articleNumber, cancellationToken);
        if (existing is not null)
            throw new InvalidOperationException("Diese Artikelnummer ist bereits vorhanden.");

        var item = new MaterialItem(
            articleNumber,
            description,
            unit,
            purchasePrice);

        item.Update(
            articleNumber,
            description,
            unit,
            purchasePrice,
            supplier,
            supplierArticleNumber);

        await repository.AddAsync(item, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        await auditTrail.WriteAsync(
            "MaterialItem",
            item.Id,
            "Created",
            $"Materialartikel {item.ArticleNumber} wurde angelegt.",
            cancellationToken);

        return Map(item);
    }

    public async Task UpdateAsync(
        Guid id,
        string articleNumber,
        string description,
        string unit,
        decimal purchasePrice,
        string? supplier,
        string? supplierArticleNumber,
        CancellationToken cancellationToken = default)
    {
        var item = await GetRequiredAsync(id, cancellationToken);
        var duplicate = await repository.FindByArticleNumberAsync(articleNumber, cancellationToken);

        if (duplicate is not null && duplicate.Id != id)
            throw new InvalidOperationException("Diese Artikelnummer ist bereits vorhanden.");

        item.Update(
            articleNumber,
            description,
            unit,
            purchasePrice,
            supplier,
            supplierArticleNumber);

        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task SetActiveAsync(
        Guid id,
        bool active,
        CancellationToken cancellationToken = default)
    {
        var item = await GetRequiredAsync(id, cancellationToken);

        if (active)
            item.Activate();
        else
            item.Deactivate();

        await repository.SaveChangesAsync(cancellationToken);
    }

    private async Task<MaterialItem> GetRequiredAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        await repository.GetAsync(id, cancellationToken)
        ?? throw new InvalidOperationException("Materialartikel wurde nicht gefunden.");

    private static MaterialItemDto Map(MaterialItem x) => new(
        x.Id,
        x.ArticleNumber,
        x.Description,
        x.Unit,
        x.PurchasePrice,
        x.Supplier,
        x.SupplierArticleNumber,
        x.PriceUpdatedAtUtc,
        x.IsActive,
        DateTimeOffset.UtcNow - x.PriceUpdatedAtUtc > TimeSpan.FromDays(90),
        Math.Max(0, (int)(DateTimeOffset.UtcNow - x.PriceUpdatedAtUtc).TotalDays));
}
