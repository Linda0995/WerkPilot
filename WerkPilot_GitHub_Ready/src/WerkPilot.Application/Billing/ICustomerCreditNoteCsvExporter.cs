namespace WerkPilot.Application.Billing;

public interface ICustomerCreditNoteCsvExporter
{
    string Export(CustomerCreditNoteDto creditNote);
}
