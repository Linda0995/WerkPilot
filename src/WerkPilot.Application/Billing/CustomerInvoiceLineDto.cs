namespace WerkPilot.Application.Billing;

public sealed record CustomerInvoiceLineDto(
    Guid Id,
    string Description,
    decimal Quantity,
    string Unit,
    decimal UnitPriceNet,
    decimal VatRatePercent,
    decimal NetTotal,
    decimal VatAmount,
    decimal GrossTotal);
