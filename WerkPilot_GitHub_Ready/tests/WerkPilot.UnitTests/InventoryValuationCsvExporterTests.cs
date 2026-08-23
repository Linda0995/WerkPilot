using WerkPilot.Application.Inventory;
using WerkPilot.Infrastructure.Inventory;

namespace WerkPilot.UnitTests;

public sealed class InventoryValuationCsvExporterTests
{
    [Fact]
    public void Export_ContainsSummaryAndPriceWarning()
    {
        var item = new InventoryValuationItemDto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "MAT-001",
            "Stahlblech",
            "kg",
            "A-01",
            10m,
            2m,
            8m,
            3m,
            30m,
            6m,
            24m,
            true,
            120);

        var summary = new InventoryValuationSummaryDto(
            30m,
            6m,
            24m,
            1,
            1,
            [item]);

        var csv = new InventoryValuationCsvExporter().Export(summary);

        Assert.Contains("Gesamter Lagerwert", csv);
        Assert.Contains("Stahlblech", csv);
        Assert.Contains("Preis prüfen", csv);
        Assert.Contains("Ja", csv);
    }
}
