using WerkPilot.Application.Work;
using WerkPilot.Domain.Identity;

namespace WerkPilot.UnitTests;

public sealed class TeamWorkSummaryTests
{
    [Fact]
    public void UserSummary_PreservesWorkloadCounters()
    {
        var user = new TeamWorkUserSummaryDto(
            Guid.NewGuid(),
            "linda",
            "Linda",
            UserRole.Production,
            7,
            2,
            1,
            1,
            5,
            2,
            DateTimeOffset.UtcNow,
            []);

        Assert.Equal(7, user.OpenCount);
        Assert.Equal(5, user.CustomerFollowUpCount);
        Assert.Equal(2, user.ProjectTaskCount);
    }

    [Fact]
    public void TeamSummary_PreservesTotals()
    {
        var summary = new TeamWorkSummaryDto(
            3,
            12,
            4,
            2,
            1,
            []);

        Assert.Equal(3, summary.ActiveUserCount);
        Assert.Equal(12, summary.OpenCount);
        Assert.Equal(2, summary.OverdueCount);
    }
}
