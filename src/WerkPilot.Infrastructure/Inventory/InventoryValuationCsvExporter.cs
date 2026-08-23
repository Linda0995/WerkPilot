using System.Globalization;
using System.Text;
using WerkPilot.Application.Inventory;

namespace WerkPilot.Infrastructure.Inventory;

public sealed class InventoryValuationCsvExporter
    : IInventoryValuationCsvExporter
{
    private static readonly CultureInfo Culture =
        CultureInfo.GetCultureInfo("de-AT");

    public string Export(InventoryValuationSummaryDto summary)
    {
        var builder = new StringBuilder();

        builder.AppendLine($"Gesamter Lagerwert;{Money(summary.TotalStockValue)}");
        builder.AppendLine($"Wert reservierter Bestand;{Money(summary.TotalReservedValue)}");
        builder.AppendLine($"Wert verfügbarer Bestand;{Money(summary.TotalAvailableValue)}");
        builder.AppendLine($"Artikelanzahl;{summary.InventoryItemCount}");
        builder.AppendLine($"Artikel mit veraltetem Preis;{summary.OutdatedPriceCount}");
        builder.AppendLine();
        builder.AppendLine(
            "Artikelnummer;Beschreibung;Lagerort;Bestand;Reserviert;Verfügbar;Einheit;Einkaufspreis;Lagerwert;Reservierungswert;Verfügbarer Wert;Preisalter Tage;Preis prüfen");

        foreach (var item in summary.Items)
        {
            builder.Append(Escape(item.ArticleNumber)).Append(';')
                .Append(Escape(item.Description)).Append(';')
                .Append(Escape(item.StorageLocation)).Append(';')
                .Append(Number(item.QuantityOnHand)).Append(';')
                .Append(Number(item.ReservedQuantity)).Append(';')
                .Append(Number(item.AvailableQuantity)).Append(';')
                .Append(Escape(item.Unit)).Append(';')
                .Append(item.PurchasePrice.ToString("0.0000", Culture)).Append(';')
                .Append(Money(item.StockValue)).Append(';')
                .Append(Money(item.ReservedValue)).Append(';')
                .Append(Money(item.AvailableValue)).Append(';')
                .Append(item.PriceAgeDays).Append(';')
                .Append(item.IsPriceOutdated ? "Ja" : "Nein")
                .AppendLine();
        }

        return builder.ToString();
    }

    private static string Number(decimal value) =>
        value.ToString("0.###", Culture);

    private static string Money(decimal value) =>
        value.ToString("0.00", Culture);

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value.Contains(';') || value.Contains('"') || value.Contains('\n')
            ? $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\""
            : value;
    }
}
