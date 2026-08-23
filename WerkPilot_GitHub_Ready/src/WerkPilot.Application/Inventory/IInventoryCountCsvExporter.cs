namespace WerkPilot.Application.Inventory;

public interface IInventoryCountCsvExporter
{
    string Export(InventoryCountDto count);
}
