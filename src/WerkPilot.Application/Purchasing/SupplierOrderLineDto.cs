namespace WerkPilot.Application.Purchasing;

public sealed record SupplierOrderLineDto(
    Guid Id,
    Guid MaterialItemId,
    string ArticleNumber,
    string Description,
    string Unit,
    decimal OrderedQuantity,
    decimal ReceivedQuantity,
    decimal OpenQuantity,
    decimal UnitPriceNet,
    decimal LineTotalNet);
