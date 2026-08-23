using WerkPilot.Application.Inventory;
using WerkPilot.Infrastructure.Inventory;

namespace WerkPilot.UnitTests;

public sealed class ReorderSuggestionCsvExporterTests
{
    [Fact]
    public void Export_ContainsSupplierDemandAndPriceWarning()
    {
        var items = new[]
        {
            new ReorderSuggestionDto(
                Guid.NewGuid(), Guid.NewGuid(), "MAT-001", "Stahlblech", "kg",
                "Stahl GmbH", "S355-01", 5m, 2m, 3m, 10m, 2m,
                9m, 3m, 27m, true)
        };

        var csv = new ReorderSuggestionCsvExporter().Export(items);

        Assert.Contains("Stahl GmbH", csv);
        Assert.Contains("Offener Bedarf", csv);
        Assert.Contains("Preis prüfen", csv);
        Assert.Contains("Ja", csv);
    }
}
