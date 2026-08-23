using WerkPilot.Application.Purchasing;
using WerkPilot.Domain.Purchasing;
using WerkPilot.Infrastructure.Purchasing;

namespace WerkPilot.UnitTests;

public sealed class PurchaseListCsvExporterTests
{
    [Fact]
    public void Export_ContainsSupplierAndOrderState()
    {
        var dto = new PurchaseListDto(
            Guid.NewGuid(),
            "BL-2026-0001",
            Guid.NewGuid(),
            "Test",
            PurchaseListStatus.InProgress,
            1,
            0,
            25m,
            [
                new PurchaseListItemDto(
                    Guid.NewGuid(),
                    1,
                    Guid.NewGuid(),
                    "MAT-001",
                    "Stahl",
                    "kg",
                    10m,
                    2.50m,
                    25m,
                    "Stahl GmbH",
                    true,
                    DateTimeOffset.UtcNow,
                    "Telefonisch bestellt")
            ]);

        var csv = new SemicolonPurchaseListCsvExporter().Export(dto);

        Assert.Contains("Stahl GmbH", csv);
        Assert.Contains("Telefonisch bestellt", csv);
        Assert.Contains("Ja", csv);
    }
}
