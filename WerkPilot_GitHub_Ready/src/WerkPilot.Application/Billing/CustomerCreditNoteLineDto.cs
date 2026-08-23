namespace WerkPilot.Application.Billing;

public sealed record CustomerCreditNoteLineDto(
    Guid Id,
    Guid? SourceInvoiceLineId,
    string Description,
    decimal Quantity,
    string Unit,
    decimal UnitPriceNet,
    decimal VatRatePercent,
    decimal NetTotal,
    decimal VatAmount,
    decimal GrossTotal);
