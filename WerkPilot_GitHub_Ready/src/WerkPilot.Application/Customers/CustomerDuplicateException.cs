namespace WerkPilot.Application.Customers;

public sealed class CustomerDuplicateException(IReadOnlyList<CustomerDuplicateDto> duplicates)
    : Exception("Es wurden mögliche Kundendubletten gefunden.")
{
    public IReadOnlyList<CustomerDuplicateDto> Duplicates { get; } = duplicates;
}
