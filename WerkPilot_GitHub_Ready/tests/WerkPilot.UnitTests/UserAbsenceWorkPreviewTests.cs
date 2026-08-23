using WerkPilot.Application.Identity;

namespace WerkPilot.UnitTests;

public sealed class UserAbsenceWorkPreviewTests
{
    [Fact]
    public void Preview_PreservesAffectedWorkCounters()
    {
        var items = new[]
        {
            new UserAbsenceAffectedWorkItemDto(
                UserAbsenceAffectedWorkType.CustomerFollowUp,
                Guid.NewGuid(),
                null,
                "KD-0001",
                "Muster GmbH",
                "Kunde anrufen",
                "High",
                new DateOnly(2026, 8, 12),
                true),
            new UserAbsenceAffectedWorkItemDto(
                UserAbsenceAffectedWorkType.ProjectTask,
                Guid.NewGuid(),
                Guid.NewGuid(),
                "PR-0001",
                "Umbau",
                "Montage",
                "Normal",
                new DateOnly(2026, 8, 20),
                false)
        };

        var preview = new UserAbsenceWorkPreviewDto(
            Guid.NewGuid(),
            "Linda",
            "Stephan",
            2,
            1,
            items);

        Assert.Equal(2, preview.TotalOpenCount);
        Assert.Equal(1, preview.DueDuringAbsenceCount);
    }

    [Fact]
    public void AffectedWork_TypeText_IsReadable()
    {
        var item = new UserAbsenceAffectedWorkItemDto(
            UserAbsenceAffectedWorkType.ProjectTask,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "PR-0001",
            "Umbau",
            "Montage",
            "Normal",
            null,
            false);

        Assert.Equal("Projekt-Aufgabe", item.TypeText);
    }
}
