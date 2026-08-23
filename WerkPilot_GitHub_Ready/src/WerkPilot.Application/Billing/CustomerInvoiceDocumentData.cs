using WerkPilot.Application.Settings;
namespace WerkPilot.Application.Billing;
public sealed record CustomerInvoiceDocumentData(
    CustomerInvoiceDto Invoice,
    CompanyProfileDto Company);
