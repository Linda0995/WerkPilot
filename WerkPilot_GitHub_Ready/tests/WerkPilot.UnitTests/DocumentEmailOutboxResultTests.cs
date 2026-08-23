using WerkPilot.Application.Messaging;

namespace WerkPilot.UnitTests;

public sealed class DocumentEmailOutboxResultTests
{
    [Fact]
    public void Result_PreservesProcessingCounts()
    {
        var result = new DocumentEmailOutboxResult(
            5,
            4,
            1,
            DateTimeOffset.UtcNow);

        Assert.Equal(5, result.DueCount);
        Assert.Equal(4, result.SentCount);
        Assert.Equal(1, result.FailedCount);
    }
}
