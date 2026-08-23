using WerkPilot.Application.Billing;
using WerkPilot.Domain.Billing;
using WerkPilot.Infrastructure.Billing;

namespace WerkPilot.UnitTests;

public sealed class CustomerInvoiceCsvExporterTests
{
    [Fact]
    public void Export_ContainsTotalsAndPaymentHistory()
    {
        var invoice = new CustomerInvoiceDto(
            Guid.NewGuid(),
            "RE-2026-0001",
            Guid.NewGuid(),
            "Muster GmbH",
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 8, 15),
            null,
            null,
            "Max",
            CustomerInvoiceStatus.PartiallyPaid,
            DunningLevel.None,
            null,
            DateTimeOffset.UtcNow,
            null,
            100m,
            20m,
            120m,
            60m,
            0m,
            60m,
            false,
            0,
            [
                new CustomerInvoiceLineDto(
                    Guid.NewGuid(),
                    "Leistung",
                    1m,
                    "Pauschal",
                    100m,
                    20m,
                    100m,
                    20m,
                    120m)
            ],
            [
                new CustomerInvoicePaymentDto(
                    Guid.NewGuid(),
                    60m,
                    new DateOnly(2026, 8, 5),
                    "BANK-1",
                    "Max",
                    DateTimeOffset.UtcNow)
            ]);

        var csv = new CustomerInvoiceCsvExporter().Export(invoice);

        Assert.Contains("Brutto", csv);
        Assert.Contains("Offen", csv);
        Assert.Contains("Zahlungsdatum", csv);
        Assert.Contains("Muster GmbH", csv);
    }
}
