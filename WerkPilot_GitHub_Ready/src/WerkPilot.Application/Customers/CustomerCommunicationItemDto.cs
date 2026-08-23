namespace WerkPilot.Application.Customers;

public sealed record CustomerCommunicationItemDto(
    CustomerCommunicationType Type,
    Guid DocumentId,
    string DocumentNumber,
    string Title,
    string Recipient,
    string Status,
    DateTimeOffset OccurredAtUtc,
    string? ErrorMessage)
{
    public string TypeText => Type switch
    {
        CustomerCommunicationType.OfferEmail => "Angebot",
        CustomerCommunicationType.InvoiceEmail => "Rechnung",
        CustomerCommunicationType.CreditNoteEmail => "Gutschrift",
        CustomerCommunicationType.DunningEmail => "Mahnung",
        _ => Type.ToString()
    };
}
