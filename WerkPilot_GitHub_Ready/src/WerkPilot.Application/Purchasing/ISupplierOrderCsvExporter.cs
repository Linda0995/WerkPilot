namespace WerkPilot.Application.Purchasing;

public interface ISupplierOrderCsvExporter
{
    string Export(SupplierOrderDto order);
}
