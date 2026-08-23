using WerkPilot.Domain.Customers;

namespace WerkPilot.Application.Customers;

public sealed record CustomerDto(
    Guid Id,
    string CustomerNumber,
    string DisplayName,
    CustomerType Type,
    string? ContactPerson,
    string? BillingStreet,
    string? BillingPostalCode,
    string? BillingCity,
    string BillingCountryCode,
    string? DeliveryStreet,
    string? DeliveryPostalCode,
    string? DeliveryCity,
    string DeliveryCountryCode,
    string? Email,
    string? Phone,
    string? VatId,
    TaxProfile TaxProfile,
    string? Notes,
    bool IsFavorite,
    bool IsDeleted,
    DateTimeOffset? LastContactAtUtc,
    IReadOnlyList<CustomerContactDto> Contacts);
