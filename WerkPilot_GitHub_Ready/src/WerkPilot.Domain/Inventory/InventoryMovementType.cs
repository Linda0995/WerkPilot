namespace WerkPilot.Domain.Inventory;

public enum InventoryMovementType
{
    Receipt = 1,
    Issue = 2,
    AdjustmentIncrease = 3,
    AdjustmentDecrease = 4,
    Reservation = 5,
    ReservationRelease = 6
}
