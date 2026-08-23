using WerkPilot.Application.Purchasing;
using WerkPilot.Domain.Purchasing;
using WerkPilot.Infrastructure.Purchasing;

namespace WerkPilot.UnitTests;

public sealed class SupplierInvoiceCsvExporterTests
{
    [Fact]
    public void Export_ContainsThreeWayMatchData()
    {
        var invoice = new SupplierInvoiceDto(
            Guid.NewGuid(),
            "RE-100",
            Guid.NewGuid(),
            "BE-2026-0001",
            "Stahl GmbH",
            new DateOnly(2026, 8, 2),
            new DateOnly(2026, 8, 16),
            "Max",
            SupplierInvoiceStatus.UnderReview,
            null,
            null,
            null,
            null,
            2m,
            new DateOnly(2026, 8, 10),
            0.62m,
            30.38m,
            31m,
            0m,
            31m,
            1m,
            1,
            0,
            SupplierInvoiceMatchStatus.Warning,
            [
                new SupplierInvoiceLineDto(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    "MAT-001",
                    "Stahlblech",
                    10m,
                    10m,
                    10m,
                    0m,
                    3m,
                    3.10m,
                    0m,
                    0.10m,
                    1m,
                    31m,
                    SupplierInvoiceMatchStatus.Warning)
            ],
            []);

        var csv = new SupplierInvoiceCsvExporter().Export(invoice);

        Assert.Contains("Bestellnummer", csv);
        Assert.Contains("Bestellt", csv);
        Assert.Contains("Wareneingang", csv);
        Assert.Contains("Rechnungspreis", csv);
        Assert.Contains("Warnungen", csv);
    }
}
