namespace WerkPilot.Application.Customers;

public sealed record AddCustomerContactRequest(
    Guid CustomerId,
    string Label,
    string? Email,
    string? Phone,
    bool IsPrimary);
