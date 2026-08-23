namespace WerkPilot.Application.Customers;

public sealed record CustomerContactDto(
    Guid Id,
    string Label,
    string? Email,
    string? Phone,
    bool IsPrimary);
