using WerkPilot.Application.Inventory;
using WerkPilot.Domain.Inventory;
using WerkPilot.Infrastructure.Inventory;

namespace WerkPilot.UnitTests;

public sealed class InventoryCountCsvExporterTests
{
    [Fact]
    public void Export_ContainsCountNumberAndDifference()
    {
        var dto = new InventoryCountDto(
            Guid.NewGuid(),
            "INV-2026-0001",
            "Jahresinventur",
            new DateOnly(2026, 8, 2),
            "A-01",
            "Max",
            InventoryCountStatus.ReadyForPosting,
            null,
            null,
            1,
            0,
            2m,
            25m,
            1,
            [
                new InventoryCountLineDto(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    "MAT-001",
                    "Stahlblech",
                    "kg",
                    "A-01",
                    10m,
                    8m,
                    -2m,
                    12.50m,
                    -25m,
                    true,
                    "Differenz geprüft",
                    "Max",
                    DateTimeOffset.UtcNow,
                    true)
            ]);

        var csv = new InventoryCountCsvExporter().Export(dto);

        Assert.Contains("INV-2026-0001", csv);
        Assert.Contains("Stahlblech", csv);
        Assert.Contains("Differenz", csv);
    }
}
