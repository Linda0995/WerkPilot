using WerkPilot.Domain.Messaging;

namespace WerkPilot.UnitTests;

public sealed class DocumentEmailDispatchTests
{
    [Fact]
    public void MarkSent_SetsStatusAndTimestamp()
    {
        var dispatch = CreateDispatch();

        dispatch.BeginAttempt();
        dispatch.MarkSent();

        Assert.Equal(DocumentEmailStatus.Sent, dispatch.Status);
        Assert.NotNull(dispatch.SentAtUtc);
        Assert.Null(dispatch.ErrorMessage);
    }

    [Fact]
    public void MarkFailed_PreservesError()
    {
        var dispatch = CreateDispatch();

        dispatch.MarkFailed("SMTP nicht erreichbar");

        Assert.Equal(DocumentEmailStatus.Failed, dispatch.Status);
        Assert.Equal("SMTP nicht erreichbar", dispatch.ErrorMessage);
    }

    private static DocumentEmailDispatch CreateDispatch() =>
        new(
            DocumentEmailType.CustomerInvoice,
            Guid.NewGuid(),
            "RE-2026-0001",
            "kunde@example.com",
            "Rechnung RE-2026-0001",
            "Anbei erhalten Sie die Rechnung.",
            "RE-2026-0001.pdf",
            "Max");
}
