using WerkPilot.Domain.Common;

namespace WerkPilot.Domain.Customers;

public sealed class Customer : Entity
{
    private readonly List<CustomerContact> _contacts = [];
    private Customer() { }

    public Customer(string customerNumber, string displayName, CustomerType type)
    {
        SetCustomerNumber(customerNumber);
        Rename(displayName);
        Type = type;
    }

    public string CustomerNumber { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public CustomerType Type { get; private set; }
    public string? ContactPerson { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? VatId { get; private set; }
    public TaxProfile TaxProfile { get; private set; } = TaxProfile.Inland;
    public string? Notes { get; private set; }
    public bool IsFavorite { get; private set; }
    public DateTimeOffset? LastContactAtUtc { get; private set; }
    public Address? BillingAddress { get; private set; }
    public Address? DeliveryAddress { get; private set; }
    public IReadOnlyCollection<CustomerContact> Contacts => _contacts.AsReadOnly();

    public void UpdateMasterData(string displayName, CustomerType type)
    {
        Rename(displayName);
        Type = type;
        Touch();
    }

    public void Rename(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Der Kundenname ist erforderlich.", nameof(value));

        DisplayName = value.Trim();
        Touch();
    }

    public void UpdatePrimaryContact(string? contactPerson, string? email, string? phone)
    {
        ContactPerson = Clean(contactPerson);
        Email = Clean(email);
        Phone = Clean(phone);
        Touch();
    }

    public void SetAddresses(Address? billingAddress, Address? deliveryAddress)
    {
        BillingAddress = billingAddress;
        DeliveryAddress = deliveryAddress ?? billingAddress;
        Touch();
    }

    public CustomerContact AddContact(string label, string? email, string? phone, bool isPrimary = false)
    {
        if (isPrimary)
            foreach (var existing in _contacts)
                existing.SetPrimary(false);

        var contact = new CustomerContact(label, email, phone, isPrimary);
        _contacts.Add(contact);
        Touch();
        return contact;
    }

    public void RemoveContact(Guid contactId)
    {
        var contact = _contacts.SingleOrDefault(x => x.Id == contactId)
            ?? throw new InvalidOperationException("Ansprechpartner wurde nicht gefunden.");

        _contacts.Remove(contact);

        if (contact.IsPrimary && _contacts.Count > 0)
            _contacts[0].SetPrimary(true);

        Touch();
    }

    public void SetPrimaryContact(Guid contactId)
    {
        var selected = _contacts.SingleOrDefault(x => x.Id == contactId)
            ?? throw new InvalidOperationException("Ansprechpartner wurde nicht gefunden.");

        foreach (var contact in _contacts)
            contact.SetPrimary(contact.Id == selected.Id);

        ContactPerson = selected.Label;
        Email = selected.Email;
        Phone = selected.Phone;
        Touch();
    }

    public void UpdateTax(string? vatId, TaxProfile taxProfile)
    {
        VatId = Clean(vatId)?.ToUpperInvariant();
        TaxProfile = taxProfile;
        Touch();
    }

    public void UpdateNotes(string? notes)
    {
        Notes = Clean(notes);
        Touch();
    }

    public void SetFavorite(bool value)
    {
        IsFavorite = value;
        Touch();
    }

    public void RegisterContact(DateTimeOffset occurredAtUtc)
    {
        if (!LastContactAtUtc.HasValue || occurredAtUtc > LastContactAtUtc.Value)
            LastContactAtUtc = occurredAtUtc;

        Touch();
    }

    private void SetCustomerNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Die Kundennummer ist erforderlich.", nameof(value));

        CustomerNumber = value.Trim();
    }

    private void Touch() => UpdatedAtUtc = DateTimeOffset.UtcNow;
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
