using WerkPilot.Application.Identity;

namespace WerkPilot.UnitTests;

public sealed class UserAbsenceTransferResultTests
{
    [Fact]
    public void Result_PreservesTransferScopeAndCounts()
    {
        var result = new UserAbsenceTransferResult(
            "Linda",
            "Stephan",
            true,
            2,
            3,
            5);

        Assert.True(result.OnlyDueDuringAbsence);
        Assert.Equal(5, result.TotalTransferred);
    }
}
