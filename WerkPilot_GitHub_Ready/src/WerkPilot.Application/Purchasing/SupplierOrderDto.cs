using WerkPilot.Domain.Purchasing;

namespace WerkPilot.Application.Purchasing;

public sealed record SupplierOrderDto(
    Guid Id,
    string OrderNumber,
    string SupplierName,
    string? SupplierReference,
    DateOnly OrderDate,
    DateOnly? ExpectedDeliveryDate,
    string? CreatedBy,
    SupplierOrderStatus Status,
    DateTimeOffset? OrderedAtUtc,
    DateTimeOffset? ReceivedAtUtc,
    decimal TotalNet,
    decimal OpenQuantity,
    IReadOnlyList<SupplierOrderLineDto> Lines);
