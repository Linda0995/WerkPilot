using System.Globalization;
using System.Text;
using WerkPilot.Application.Inventory;

namespace WerkPilot.Infrastructure.Inventory;

public sealed class InventoryCountCsvExporter : IInventoryCountCsvExporter
{
    private static readonly CultureInfo Culture =
        CultureInfo.GetCultureInfo("de-AT");

    public string Export(InventoryCountDto count)
    {
        var builder = new StringBuilder();

        builder.AppendLine($"Inventurnummer;{Escape(count.CountNumber)}");
        builder.AppendLine($"Bezeichnung;{Escape(count.Title)}");
        builder.AppendLine($"Inventurdatum;{count.CountDate:dd.MM.yyyy}");
        builder.AppendLine($"Status;{count.Status}");
        builder.AppendLine($"Absoluter Differenzwert;{count.AbsoluteDifferenceValue.ToString("0.00", Culture)}");
        builder.AppendLine($"Positionen mit veraltetem Preis;{count.OutdatedPriceCount}");
        builder.AppendLine();
        builder.AppendLine(
            "Artikelnummer;Beschreibung;Lagerort;Sollbestand;Gezählter Bestand;Differenz;Einheit;Einkaufspreis;Differenzwert;Preis prüfen;Gezählt von;Gezählt am;Notiz");

        foreach (var line in count.Lines)
        {
            builder.Append(Escape(line.ArticleNumber)).Append(';')
                .Append(Escape(line.Description)).Append(';')
                .Append(Escape(line.StorageLocation)).Append(';')
                .Append(Number(line.ExpectedQuantity)).Append(';')
                .Append(line.CountedQuantity.HasValue
                    ? Number(line.CountedQuantity.Value)
                    : string.Empty).Append(';')
                .Append(Number(line.DifferenceQuantity)).Append(';')
                .Append(Escape(line.Unit)).Append(';')
                .Append(line.PurchasePrice.ToString("0.0000", Culture)).Append(';')
                .Append(line.DifferenceValue.ToString("0.00", Culture)).Append(';')
                .Append(line.IsPriceOutdated ? "Ja" : "Nein").Append(';')
                .Append(Escape(line.CountedBy)).Append(';')
                .Append(line.CountedAtUtc.HasValue
                    ? line.CountedAtUtc.Value.ToLocalTime().ToString("dd.MM.yyyy HH:mm", Culture)
                    : string.Empty).Append(';')
                .Append(Escape(line.Note))
                .AppendLine();
        }

        return builder.ToString();
    }

    private static string Number(decimal value) =>
        value.ToString("0.###", Culture);

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value.Contains(';') || value.Contains('"') || value.Contains('\n')
            ? $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\""
            : value;
    }
}
