using WerkPilot.Domain.Common;

namespace WerkPilot.Domain.Inventory;

public sealed class InventoryItem : Entity
{
    private InventoryItem() { }

    public InventoryItem(
        Guid materialItemId,
        string storageLocation,
        decimal minimumStock)
    {
        if (materialItemId == Guid.Empty)
            throw new ArgumentException("Materialartikel erforderlich.", nameof(materialItemId));
        if (minimumStock < 0)
            throw new ArgumentOutOfRangeException(nameof(minimumStock));

        MaterialItemId = materialItemId;
        StorageLocation = Clean(storageLocation);
        MinimumStock = minimumStock;
    }

    public Guid MaterialItemId { get; private set; }
    public string? StorageLocation { get; private set; }
    public decimal QuantityOnHand { get; private set; }
    public decimal ReservedQuantity { get; private set; }
    public decimal MinimumStock { get; private set; }
    public decimal AvailableQuantity => QuantityOnHand - ReservedQuantity;
    public bool IsBelowMinimum => AvailableQuantity < MinimumStock;

    public void UpdateMasterData(string storageLocation, decimal minimumStock)
    {
        if (minimumStock < 0)
            throw new ArgumentOutOfRangeException(nameof(minimumStock));

        StorageLocation = Clean(storageLocation);
        MinimumStock = minimumStock;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void ApplyMovement(
        InventoryMovementType movementType,
        decimal quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        switch (movementType)
        {
            case InventoryMovementType.Receipt:
            case InventoryMovementType.AdjustmentIncrease:
                QuantityOnHand += quantity;
                break;

            case InventoryMovementType.Issue:
            case InventoryMovementType.AdjustmentDecrease:
                if (quantity > QuantityOnHand)
                    throw new InvalidOperationException(
                        "Die Bestandsmenge reicht für diese Abbuchung nicht aus.");

                QuantityOnHand -= quantity;
                ReservedQuantity = Math.Min(ReservedQuantity, QuantityOnHand);
                break;

            case InventoryMovementType.Reservation:
                if (quantity > AvailableQuantity)
                    throw new InvalidOperationException(
                        "Die verfügbare Menge reicht für diese Reservierung nicht aus.");

                ReservedQuantity += quantity;
                break;

            case InventoryMovementType.ReservationRelease:
                if (quantity > ReservedQuantity)
                    throw new InvalidOperationException(
                        "Es kann nicht mehr Reservierung freigegeben werden als vorhanden.");

                ReservedQuantity -= quantity;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(movementType));
        }

        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
