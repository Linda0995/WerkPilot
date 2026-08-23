namespace WerkPilot.Application.Inventory;

public interface IInventoryValuationCsvExporter
{
    string Export(InventoryValuationSummaryDto summary);
}
