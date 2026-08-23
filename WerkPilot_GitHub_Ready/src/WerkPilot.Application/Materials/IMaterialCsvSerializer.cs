namespace WerkPilot.Application.Materials;

public interface IMaterialCsvSerializer
{
    IReadOnlyList<MaterialImportRow> Parse(string csvContent);
    string Serialize(IReadOnlyList<MaterialItemDto> items);
}
