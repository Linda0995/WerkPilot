using WerkPilot.Domain.Crm;

namespace WerkPilot.UnitTests;

public sealed class CustomerInteractionTests
{
    [Fact]
    public void Constructor_CreatesOpenFollowUp()
    {
        var interaction = new CustomerInteraction(
            Guid.NewGuid(),
            CustomerInteractionType.Phone,
            "Anfrage",
            "Kunde möchte Angebot.",
            DateTimeOffset.UtcNow,
            "Max Muster",
            "admin",
            new DateOnly(2026, 8, 10),
            "Vertrieb");

        Assert.False(interaction.FollowUpCompleted);
        Assert.Equal("Vertrieb", interaction.FollowUpOwner);
    }

    [Fact]
    public void CompletingFollowUp_SetsTimestamp()
    {
        var interaction = new CustomerInteraction(
            Guid.NewGuid(),
            CustomerInteractionType.Email,
            "Nachfassen",
            "E-Mail gesendet.",
            DateTimeOffset.UtcNow,
            null,
            "admin",
            new DateOnly(2026, 8, 10),
            "Vertrieb");

        interaction.SetFollowUpCompleted(true);

        Assert.True(interaction.FollowUpCompleted);
        Assert.NotNull(interaction.FollowUpCompletedAtUtc);
    }

    [Fact]
    public void CompletingWithoutFollowUp_Throws()
    {
        var interaction = new CustomerInteraction(
            Guid.NewGuid(),
            CustomerInteractionType.Note,
            "Notiz",
            "Nur eine Notiz.",
            DateTimeOffset.UtcNow,
            null,
            "admin",
            null,
            null);

        Assert.Throws<InvalidOperationException>(() =>
            interaction.SetFollowUpCompleted(true));
    }
}
