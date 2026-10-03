using WerkPilot.Domain.Identity;

namespace WerkPilot.UnitTests;

public sealed class UserAbsenceTests
{
    [Fact]
    public void Absence_IncludesDateInsideRange()
    {
        var absence = CreateAbsence(
            new DateOnly(2026, 8, 10),
            new DateOnly(2026, 8, 14));

        absence.RefreshStatus(new DateOnly(2026, 8, 11));

        Assert.True(absence.Includes(new DateOnly(2026, 8, 12)));
        Assert.Equal(UserAbsenceStatus.Active, absence.Status);
    }

    [Fact]
    public void Absence_DetectsOverlap()
    {
        var absence = CreateAbsence(
            new DateOnly(2026, 8, 10),
            new DateOnly(2026, 8, 14));

        Assert.True(absence.Overlaps(
            new DateOnly(2026, 8, 14),
            new DateOnly(2026, 8, 20)));
    }

    [Fact]
    public void UserCannotRepresentThemself()
    {
        var userId = Guid.NewGuid();

        Assert.Throws<ArgumentException>(() =>
            new UserAbsence(
                userId,
                "Linda",
                UserAbsenceType.Urlaub,
                new DateOnly(2026, 8, 10),
                new DateOnly(2026, 8, 12),
                userId,
                "Linda",
                null,
                "Admin"));
    }

    private static UserAbsence CreateAbsence(
        DateOnly start,
        DateOnly end) =>
        new(
            Guid.NewGuid(),
            "Linda",
            UserAbsenceType.Urlaub,
            start,
            end,
            Guid.NewGuid(),
            "Stephan",
            null,
            "Admin");
}
