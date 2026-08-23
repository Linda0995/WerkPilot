using WerkPilot.Application.Billing;
using WerkPilot.Domain.Billing;
using WerkPilot.Infrastructure.Billing;

namespace WerkPilot.UnitTests;

public sealed class CustomerCreditNoteCsvExporterTests
{
    [Fact]
    public void Export_ContainsInvoiceReasonAndTotals()
    {
        var dto = new CustomerCreditNoteDto(
            Guid.NewGuid(), "GS-2026-0001", Guid.NewGuid(), "RE-2026-0001",
            Guid.NewGuid(), "Muster GmbH", new DateOnly(2026, 8, 6),
            "Preisnachlass", "Max", CustomerCreditNoteStatus.Issued,
            DateTimeOffset.UtcNow, null, 25m, 5m, 30m,
            [
                new CustomerCreditNoteLineDto(
                    Guid.NewGuid(), Guid.NewGuid(), "Leistung", 1m, "Pauschal",
                    25m, 20m, 25m, 5m, 30m)
            ]);

        var csv = new CustomerCreditNoteCsvExporter().Export(dto);

        Assert.Contains("GS-2026-0001", csv);
        Assert.Contains("RE-2026-0001", csv);
        Assert.Contains("Preisnachlass", csv);
    }
}
