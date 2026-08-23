namespace WerkPilot.Domain.Inventory;

public enum InventoryCountStatus
{
    Draft = 1,
    Counting = 2,
    ReadyForPosting = 3,
    Posted = 4,
    Cancelled = 5
}
