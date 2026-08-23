namespace WerkPilot.Application.Purchasing;

public sealed record SupplierInvoiceLineDto(
    Guid Id,
    Guid SupplierOrderLineId,
    Guid MaterialItemId,
    string ArticleNumber,
    string Description,
    decimal OrderedQuantity,
    decimal ReceivedQuantity,
    decimal InvoicedQuantity,
    decimal OpenReceiptQuantity,
    decimal OrderedUnitPriceNet,
    decimal InvoicedUnitPriceNet,
    decimal QuantityVariance,
    decimal PriceVariancePerUnit,
    decimal ValueVariance,
    decimal LineTotalNet,
    SupplierInvoiceMatchStatus MatchStatus);
