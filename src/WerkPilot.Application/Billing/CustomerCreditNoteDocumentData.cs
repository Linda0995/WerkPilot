using WerkPilot.Application.Settings;
namespace WerkPilot.Application.Billing;
public sealed record CustomerCreditNoteDocumentData(
    CustomerCreditNoteDto CreditNote,
    CompanyProfileDto Company);
