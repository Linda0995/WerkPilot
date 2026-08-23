namespace WerkPilot.Domain.Billing;

public enum CustomerInvoiceStatus
{
    Draft = 1,
    Issued = 2,
    PartiallyPaid = 3,
    Paid = 4,
    Cancelled = 5
}
