using WerkPilot.Application.Identity;

namespace WerkPilot.UnitTests;

public sealed class UserAbsenceConflictDtoTests
{
    [Fact]
    public void MissingSubstituteWithOpenWork_RequiresAction()
    {
        var conflict = new UserAbsenceConflictDto(
            Guid.NewGuid(),
            "Linda",
            3,
            2,
            4,
            false);

        Assert.Equal(5, conflict.OpenWorkCount);
        Assert.True(conflict.RequiresAction);
    }
}
