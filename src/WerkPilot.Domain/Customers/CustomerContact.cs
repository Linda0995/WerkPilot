namespace WerkPilot.Domain.Customers;

public sealed class CustomerContact
{
    private CustomerContact() { }

    public CustomerContact(string label, string? email, string? phone, bool isPrimary)
    {
        Id = Guid.NewGuid();
        Update(label, email, phone);
        IsPrimary = isPrimary;
    }

    public Guid Id { get; private init; }
    public string Label { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public bool IsPrimary { get; private set; }

    public void Update(string label, string? email, string? phone)
    {
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Die Bezeichnung des Ansprechpartners ist erforderlich.", nameof(label));

        Label = label.Trim();
        Email = Clean(email);
        Phone = Clean(phone);
    }

    internal void SetPrimary(bool value) => IsPrimary = value;

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
