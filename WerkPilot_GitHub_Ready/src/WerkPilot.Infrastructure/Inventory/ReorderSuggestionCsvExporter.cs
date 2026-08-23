using System.Globalization;
using System.Text;
using WerkPilot.Application.Inventory;

namespace WerkPilot.Infrastructure.Inventory;

public sealed class ReorderSuggestionCsvExporter
    : IReorderSuggestionCsvExporter
{
    private static readonly CultureInfo Culture =
        CultureInfo.GetCultureInfo("de-AT");

    public string Export(IReadOnlyList<ReorderSuggestionDto> suggestions)
    {
        var builder = new StringBuilder();

        builder.AppendLine(
            "Lieferant;Lieferantenartikel;Artikelnummer;Beschreibung;Bestand;Reserviert;Verfügbar;Offener Bedarf;Mindestbestand;Bestellvorschlag;Einheit;Einkaufspreis;Bestellwert;Preis prüfen");

        foreach (var item in suggestions)
        {
            builder.Append(Escape(item.Supplier)).Append(';')
                .Append(Escape(item.SupplierArticleNumber)).Append(';')
                .Append(Escape(item.ArticleNumber)).Append(';')
                .Append(Escape(item.Description)).Append(';')
                .Append(Number(item.QuantityOnHand)).Append(';')
                .Append(Number(item.ReservedQuantity)).Append(';')
                .Append(Number(item.AvailableQuantity)).Append(';')
                .Append(Number(item.OpenDemandQuantity)).Append(';')
                .Append(Number(item.MinimumStock)).Append(';')
                .Append(Number(item.SuggestedOrderQuantity)).Append(';')
                .Append(Escape(item.Unit)).Append(';')
                .Append(item.PurchasePrice.ToString("0.0000", Culture)).Append(';')
                .Append(item.EstimatedOrderValue.ToString("0.00", Culture)).Append(';')
                .Append(item.IsPriceOutdated ? "Ja" : "Nein")
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
