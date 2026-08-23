using WerkPilot.Domain.Inventory;

namespace WerkPilot.Application.Inventory;

public sealed record InventoryMovementDto(
    Guid Id,
    Guid InventoryItemId,
    InventoryMovementType MovementType,
    decimal Quantity,
    string Reason,
    DateTimeOffset OccurredAtUtc,
    Guid? ProjectId,
    string? Reference,
    string? CreatedBy);
