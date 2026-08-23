using System.Globalization;
using System.Text;
using WerkPilot.Application.Purchasing;

namespace WerkPilot.Infrastructure.Purchasing;

public sealed class SemicolonPurchaseListCsvExporter : IPurchaseListCsvExporter
{
    private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("de-AT");

    public string Export(PurchaseListDto purchaseList)
    {
        var builder = new StringBuilder();

        builder.AppendLine($"Bestellliste;{Escape(purchaseList.PurchaseListNumber)}");
        builder.AppendLine($"Titel;{Escape(purchaseList.Title)}");
        builder.AppendLine($"Status;{purchaseList.Status}");
        builder.AppendLine();
        builder.AppendLine(
            "Lieferant;Artikelnummer;Beschreibung;Menge;Einheit;Preis;Schätzwert;Bestellt;Bestelldatum;Notiz");

        foreach (var item in purchaseList.Items
                     .OrderBy(x => x.Supplier)
                     .ThenBy(x => x.PositionNumber))
        {
            builder.Append(Escape(item.Supplier)).Append(';')
                .Append(Escape(item.ArticleNumber)).Append(';')
                .Append(Escape(item.Description)).Append(';')
                .Append(item.RequiredQuantity.ToString("0.###", Culture)).Append(';')
                .Append(Escape(item.Unit)).Append(';')
                .Append(item.PurchasePrice.ToString("0.####", Culture)).Append(';')
                .Append(item.EstimatedTotal.ToString("0.00", Culture)).Append(';')
                .Append(item.IsOrdered ? "Ja" : "Nein").Append(';')
                .Append(item.OrderedAtUtc?.ToLocalTime().ToString("dd.MM.yyyy HH:mm", Culture) ?? string.Empty)
                .Append(';')
                .Append(Escape(item.OrderNote))
                .AppendLine();
        }

        return builder.ToString();
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value.Contains(';') || value.Contains('"') || value.Contains('\n')
            ? $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\""
            : value;
    }
}
