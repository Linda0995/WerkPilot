using WerkPilot.Application.Customers;

namespace WerkPilot.UnitTests;

public sealed class CustomerDuplicateTests
{
    [Fact]
    public void Exception_ExposesDuplicateInformation()
    {
        var duplicates = new[]
        {
            new CustomerDuplicateDto(
                Guid.NewGuid(),
                "K-2026-0001",
                "Muster GmbH",
                "gleiche UID/ATU")
        };

        var exception = new CustomerDuplicateException(duplicates);

        Assert.Single(exception.Duplicates);
        Assert.Contains("Kundendubletten", exception.Message);
    }
}
