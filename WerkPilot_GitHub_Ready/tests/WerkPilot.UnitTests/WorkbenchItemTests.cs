using WerkPilot.Domain.Workbench;

namespace WerkPilot.UnitTests;

public sealed class WorkbenchItemTests
{
    [Fact]
    public void Touch_UpdatesMetadataAndTimestamp()
    {
        var item = new WorkbenchItem(Guid.NewGuid(), "Project", Guid.NewGuid(), "PR-1", "Alt", null);
        var before = item.LastOpenedAtUtc;

        item.Touch("PR-1", "Neu", "Aktiv");

        Assert.Equal("Neu", item.Title);
        Assert.Equal("Aktiv", item.Subtitle);
        Assert.True(item.LastOpenedAtUtc >= before);
    }

    [Fact]
    public void Favorite_CanBeEnabledAndDisabled()
    {
        var item = new WorkbenchItem(Guid.NewGuid(), "Offer", Guid.NewGuid(), "AN-1", "Test", null);
        item.SetFavorite(true);
        Assert.True(item.IsFavorite);
        item.SetFavorite(false);
        Assert.False(item.IsFavorite);
    }
}
