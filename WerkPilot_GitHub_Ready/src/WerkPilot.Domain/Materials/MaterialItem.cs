using WerkPilot.Domain.Common;

namespace WerkPilot.Domain.Materials;

public sealed class MaterialItem : Entity
{
    private MaterialItem() { }

    public MaterialItem(
        string articleNumber,
        string description,
        string unit,
        decimal purchasePrice)
    {
        Update(articleNumber, description, unit, purchasePrice);
        IsActive = true;
    }

    public string ArticleNumber { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Unit { get; private set; } = string.Empty;
    public decimal PurchasePrice { get; private set; }
    public string? Supplier { get; private set; }
    public string? SupplierArticleNumber { get; private set; }
    public DateTimeOffset PriceUpdatedAtUtc { get; private set; } = DateTimeOffset.UtcNow;
    public bool IsActive { get; private set; }

    public void Update(
        string articleNumber,
        string description,
        string unit,
        decimal purchasePrice,
        string? supplier = null,
        string? supplierArticleNumber = null)
    {
        if (string.IsNullOrWhiteSpace(articleNumber))
            throw new ArgumentException("Artikelnummer erforderlich.", nameof(articleNumber));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Beschreibung erforderlich.", nameof(description));
        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("Einheit erforderlich.", nameof(unit));
        if (purchasePrice < 0)
            throw new ArgumentOutOfRangeException(nameof(purchasePrice));

        ArticleNumber = articleNumber.Trim();
        Description = description.Trim();
        Unit = unit.Trim();
        PurchasePrice = purchasePrice;
        Supplier = Clean(supplier);
        SupplierArticleNumber = Clean(supplierArticleNumber);
        PriceUpdatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public bool IsPriceOutdated(int maximumAgeDays)
    {
        if (maximumAgeDays < 0)
            throw new ArgumentOutOfRangeException(nameof(maximumAgeDays));

        return DateTimeOffset.UtcNow - PriceUpdatedAtUtc
            > TimeSpan.FromDays(maximumAgeDays);
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
