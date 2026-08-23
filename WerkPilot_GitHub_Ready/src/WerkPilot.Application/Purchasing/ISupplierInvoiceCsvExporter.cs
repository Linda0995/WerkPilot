namespace WerkPilot.Application.Purchasing;

public interface ISupplierInvoiceCsvExporter
{
    string Export(SupplierInvoiceDto invoice);
}
