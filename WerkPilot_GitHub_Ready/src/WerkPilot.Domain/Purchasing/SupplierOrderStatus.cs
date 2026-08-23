namespace WerkPilot.Domain.Purchasing;

public enum SupplierOrderStatus
{
    Draft = 1,
    Ordered = 2,
    PartiallyReceived = 3,
    Received = 4,
    Cancelled = 5
}
