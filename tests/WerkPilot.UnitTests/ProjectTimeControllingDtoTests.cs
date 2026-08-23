using WerkPilot.Application.TimeTracking;

namespace WerkPilot.UnitTests;

public sealed class ProjectTimeControllingDtoTests
{
    [Fact]
    public void Dto_PreservesBudgetAndVariance()
    {
        var dto = new ProjectTimeControllingDto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            100m,
            5000m,
            50m,
            85m,
            4250m,
            15m,
            -15m,
            85m,
            ProjectTimeControllingStatus.Warning);

        Assert.Equal(100m, dto.PlannedLaborHours);
        Assert.Equal(85m, dto.ActualHours);
        Assert.Equal(15m, dto.RemainingHours);
        Assert.Equal(ProjectTimeControllingStatus.Warning, dto.Status);
    }

    [Theory]
    [InlineData(ProjectTimeControllingStatus.NoBudget)]
    [InlineData(ProjectTimeControllingStatus.OnTrack)]
    [InlineData(ProjectTimeControllingStatus.Warning)]
    [InlineData(ProjectTimeControllingStatus.Exceeded)]
    public void AllStatuses_AreDefined(ProjectTimeControllingStatus status)
    {
        Assert.True(Enum.IsDefined(status));
    }
}
