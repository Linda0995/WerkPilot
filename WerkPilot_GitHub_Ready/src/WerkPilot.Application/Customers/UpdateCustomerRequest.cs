using WerkPilot.Domain.Customers;

namespace WerkPilot.Application.Customers;

public sealed record UpdateCustomerRequest(
    Guid Id,
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
    bool DeliveryAddressEqualsBillingAddress,
    string? Email,
    string? Phone,
    string? VatId,
    TaxProfile TaxProfile,
    string? Notes);
