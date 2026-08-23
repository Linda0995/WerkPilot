namespace WerkPilot.Application.Search;

public sealed record GlobalSearchResult(
    SearchResultType Type,
    Guid EntityId,
    string PrimaryText,
    string SecondaryText,
    string SearchNumber,
    string TargetModule,
    int Relevance);
