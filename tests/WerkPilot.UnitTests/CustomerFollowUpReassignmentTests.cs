using WerkPilot.Domain.Crm;

namespace WerkPilot.UnitTests;

public sealed class CustomerFollowUpReassignmentTests
{
    [Fact]
    public void OpenFollowUp_CanBeReassigned()
    {
        var target = Guid.NewGuid();

        var followUp = new CustomerFollowUp(
            Guid.NewGuid(),
            "KD-0001",
            "Muster GmbH",
            "Kunde anrufen",
            null,
            DateTimeOffset.UtcNow.AddDays(1),
            CustomerFollowUpPriority.Normal,
            null,
            "Linda",
            "Stephan");

        followUp.Reassign(target, "Stephan");

        Assert.Equal(target, followUp.AssignedUserId);
        Assert.Equal("Stephan", followUp.AssignedTo);
    }
}
