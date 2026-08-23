namespace WerkPilot.Application.Materials;

public sealed record MaterialItemDto(
    Guid Id,
    string ArticleNumber,
    string Description,
    string Unit,
    decimal PurchasePrice,
    string? Supplier,
    string? SupplierArticleNumber,
    DateTimeOffset PriceUpdatedAtUtc,
    bool IsActive,
    bool IsPriceOutdated,
    int PriceAgeDays);
