namespace WerkPilot.Application.Customers;

public sealed record CustomerDuplicateDto(
    Guid Id,
    string CustomerNumber,
    string DisplayName,
    string Reason);
