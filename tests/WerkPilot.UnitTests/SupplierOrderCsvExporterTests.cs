using WerkPilot.Application.Purchasing;
using WerkPilot.Domain.Purchasing;
using WerkPilot.Infrastructure.Purchasing;

namespace WerkPilot.UnitTests;

public sealed class SupplierOrderCsvExporterTests
{
    [Fact]
    public void Export_ContainsOrderSupplierAndOpenQuantity()
    {
        var order = new SupplierOrderDto(
            Guid.NewGuid(),
            "BE-2026-0001",
            "Stahl GmbH",
            "REF-01",
            new DateOnly(2026, 8, 2),
            new DateOnly(2026, 8, 9),
            "Max",
            SupplierOrderStatus.Ordered,
            DateTimeOffset.UtcNow,
            null,
            30m,
            10m,
            [
                new SupplierOrderLineDto(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    "MAT-001",
                    "Stahlblech",
                    "kg",
                    10m,
                    0m,
                    10m,
                    3m,
                    30m)
            ]);

        var csv = new SupplierOrderCsvExporter().Export(order);

        Assert.Contains("BE-2026-0001", csv);
        Assert.Contains("Stahl GmbH", csv);
        Assert.Contains("Offen", csv);
    }
}
