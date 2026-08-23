namespace WerkPilot.Application.Inventory;

public interface IReorderSuggestionCsvExporter
{
    string Export(IReadOnlyList<ReorderSuggestionDto> suggestions);
}
