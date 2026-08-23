using WerkPilot.Application.Work;

namespace WerkPilot.UnitTests;

public sealed class WorkReassignmentResultTests
{
    [Fact]
    public void Result_PreservesTransferCounters()
    {
        var result = new ReassignWorkResult(
            "Linda",
            "Stephan",
            3,
            4,
            7);

        Assert.Equal(7, result.TotalTransferred);
        Assert.Equal(3, result.CustomerFollowUpsTransferred);
        Assert.Equal(4, result.ProjectTasksTransferred);
    }
}
