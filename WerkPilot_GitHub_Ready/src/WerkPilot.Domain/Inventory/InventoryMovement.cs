using WerkPilot.Domain.Common;

namespace WerkPilot.Domain.Inventory;

public sealed class InventoryMovement : Entity
{
    private InventoryMovement() { }

    public InventoryMovement(
        Guid inventoryItemId,
        InventoryMovementType movementType,
        decimal quantity,
        string reason,
        DateTimeOffset occurredAtUtc,
        Guid? projectId,
        string? reference,
        string? createdBy)
    {
        if (inventoryItemId == Guid.Empty)
            throw new ArgumentException("Lagerartikel erforderlich.", nameof(inventoryItemId));
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Buchungsgrund erforderlich.", nameof(reason));

        InventoryItemId = inventoryItemId;
        MovementType = movementType;
        Quantity = quantity;
        Reason = reason.Trim();
        OccurredAtUtc = occurredAtUtc;
        ProjectId = projectId;
        Reference = Clean(reference);
        CreatedBy = Clean(createdBy);
    }

    public Guid InventoryItemId { get; private set; }
    public InventoryMovementType MovementType { get; private set; }
    public decimal Quantity { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public DateTimeOffset OccurredAtUtc { get; private set; }
    public Guid? ProjectId { get; private set; }
    public string? Reference { get; private set; }
    public string? CreatedBy { get; private set; }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
