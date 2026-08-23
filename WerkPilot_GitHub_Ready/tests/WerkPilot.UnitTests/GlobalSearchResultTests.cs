using WerkPilot.Application.Search;

namespace WerkPilot.UnitTests;

public sealed class GlobalSearchResultTests
{
    [Fact]
    public void Result_PreservesNavigationMetadata()
    {
        var id = Guid.NewGuid();
        var result = new GlobalSearchResult(
            SearchResultType.Project,
            id,
            "Geländerbau",
            "Aktiv · 50%",
            "PR-2026-0001",
            "Projekte",
            80);

        Assert.Equal(id, result.EntityId);
        Assert.Equal(SearchResultType.Project, result.Type);
        Assert.Equal("Projekte", result.TargetModule);
        Assert.Equal(80, result.Relevance);
    }
}
