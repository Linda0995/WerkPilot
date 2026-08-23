using WerkPilot.Domain.Billing;

namespace WerkPilot.UnitTests;

public sealed class DunningNoticeTests
{
    [Fact]
    public void TotalDue_IncludesPrincipalFeeAndInterest()
    {
        var notice = CreateNotice();

        Assert.Equal(1045m, notice.TotalDue);
    }

    [Fact]
    public void IssuedNotice_CannotBeCancelled()
    {
        var notice = CreateNotice();
        notice.Issue();

        Assert.Throws<InvalidOperationException>(() => notice.Cancel());
    }

    [Fact]
    public void Issue_SetsStatusAndTimestamp()
    {
        var notice = CreateNotice();
        notice.Issue();

        Assert.Equal(DunningNoticeStatus.Issued, notice.Status);
        Assert.NotNull(notice.IssuedAtUtc);
    }

    private static DunningNotice CreateNotice() =>
        new(
            "MA-2026-0001",
            Guid.NewGuid(),
            "RE-2026-0001",
            Guid.NewGuid(),
            "Muster GmbH",
            new DateOnly(2026, 8, 6),
            new DateOnly(2026, 8, 13),
            DunningLevel.FirstDunning,
            1000m,
            20m,
            25m,
            9.2m,
            30,
            "Max");
}
