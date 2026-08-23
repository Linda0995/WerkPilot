using System.Globalization;
using System.Text;
using WerkPilot.Application.Materials;

namespace WerkPilot.Infrastructure.Materials;

public sealed class SemicolonMaterialCsvSerializer : IMaterialCsvSerializer
{
    private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("de-AT");

    public IReadOnlyList<MaterialImportRow> Parse(string csvContent)
    {
        if (string.IsNullOrWhiteSpace(csvContent))
            throw new ArgumentException("Die CSV-Datei ist leer.", nameof(csvContent));

        var lines = csvContent
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n', StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length < 2)
            throw new FormatException("Die CSV-Datei enthält keine Datenzeilen.");

        var result = new List<MaterialImportRow>();

        for (var index = 1; index < lines.Length; index++)
        {
            var columns = SplitLine(lines[index]);

            if (columns.Count < 4)
                throw new FormatException(
                    $"Zeile {index + 1} enthält weniger als vier Pflichtspalten.");

            if (!decimal.TryParse(
                    columns[3],
                    NumberStyles.Number,
                    Culture,
                    out var purchasePrice))
                throw new FormatException(
                    $"Zeile {index + 1}: Einkaufspreis „{columns[3]}“ ist ungültig.");

            result.Add(new MaterialImportRow(
                index + 1,
                columns[0].Trim(),
                columns[1].Trim(),
                columns[2].Trim(),
                purchasePrice,
                GetOptional(columns, 4),
                GetOptional(columns, 5)));
        }

        return result;
    }

    public string Serialize(IReadOnlyList<MaterialItemDto> items)
    {
        var builder = new StringBuilder();
        builder.AppendLine(
            "Artikelnummer;Beschreibung;Einheit;Einkaufspreis;Lieferant;LieferantenArtikelnummer;Preisstand;Aktiv");

        foreach (var item in items.OrderBy(x => x.ArticleNumber))
        {
            builder.Append(Escape(item.ArticleNumber)).Append(';')
                .Append(Escape(item.Description)).Append(';')
                .Append(Escape(item.Unit)).Append(';')
                .Append(item.PurchasePrice.ToString("0.####", Culture)).Append(';')
                .Append(Escape(item.Supplier)).Append(';')
                .Append(Escape(item.SupplierArticleNumber)).Append(';')
                .Append(item.PriceUpdatedAtUtc.ToString("yyyy-MM-dd", Culture)).Append(';')
                .Append(item.IsActive ? "Ja" : "Nein")
                .AppendLine();
        }

        return builder.ToString();
    }

    private static IReadOnlyList<string> SplitLine(string line)
    {
        var values = new List<string>();
        var builder = new StringBuilder();
        var quoted = false;

        for (var index = 0; index < line.Length; index++)
        {
            var character = line[index];

            if (character == '"')
            {
                if (quoted && index + 1 < line.Length && line[index + 1] == '"')
                {
                    builder.Append('"');
                    index++;
                }
                else
                {
                    quoted = !quoted;
                }
            }
            else if (character == ';' && !quoted)
            {
                values.Add(builder.ToString());
                builder.Clear();
            }
            else
            {
                builder.Append(character);
            }
        }

        values.Add(builder.ToString());
        return values;
    }

    private static string? GetOptional(IReadOnlyList<string> columns, int index) =>
        columns.Count > index && !string.IsNullOrWhiteSpace(columns[index])
            ? columns[index].Trim()
            : null;

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value.Contains(';') || value.Contains('"') || value.Contains('\n')
            ? $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\""
            : value;
    }
}
