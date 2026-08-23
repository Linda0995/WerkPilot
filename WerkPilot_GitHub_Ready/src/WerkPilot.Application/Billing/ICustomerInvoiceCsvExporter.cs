namespace WerkPilot.Application.Billing;

public interface ICustomerInvoiceCsvExporter
{
    string Export(CustomerInvoiceDto invoice);
}
