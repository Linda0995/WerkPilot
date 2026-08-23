using WerkPilot.Domain.Messaging;

namespace WerkPilot.UnitTests;

public sealed class DocumentEmailRetryTests
{
    [Fact]
    public void FailedDispatch_CanBeScheduledForRetry()
    {
        var dispatch = CreateDispatch();
        dispatch.BeginAttempt();
        dispatch.MarkFailed("SMTP offline");

        var retryAt = DateTimeOffset.UtcNow.AddMinutes(15);
        dispatch.ScheduleRetry(retryAt);

        Assert.Equal(DocumentEmailStatus.Failed, dispatch.Status);
        Assert.Equal(retryAt, dispatch.NextRetryAtUtc);
        Assert.Equal(1, dispatch.AttemptCount);
    }

    [Fact]
    public void SuccessfulDispatch_CannotBeginAnotherAttempt()
    {
        var dispatch = CreateDispatch();
        dispatch.BeginAttempt();
        dispatch.MarkSent();

        Assert.Throws<InvalidOperationException>(() =>
            dispatch.BeginAttempt());
    }

    private static DocumentEmailDispatch CreateDispatch() =>
        new(
            DocumentEmailType.CustomerInvoice,
            Guid.NewGuid(),
            "RE-2026-0001",
            "kunde@example.com",
            "Rechnung",
            "Nachricht",
            "RE-2026-0001.pdf",
            "Max");
}
