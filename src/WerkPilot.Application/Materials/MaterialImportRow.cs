namespace WerkPilot.Application.Materials;

public sealed record MaterialImportRow(
    int RowNumber,
    string ArticleNumber,
    string Description,
    string Unit,
    decimal PurchasePrice,
    string? Supplier,
    string? SupplierArticleNumber);
