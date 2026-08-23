using System.Globalization;
using System.Text;
using WerkPilot.Application.Purchasing;

namespace WerkPilot.Infrastructure.Purchasing;

public sealed class SupplierOrderCsvExporter : ISupplierOrderCsvExporter
{
    private static readonly CultureInfo Culture =
        CultureInfo.GetCultureInfo("de-AT");

    public string Export(SupplierOrderDto order)
    {
        var builder = new StringBuilder();

        builder.AppendLine($"Bestellnummer;{Escape(order.OrderNumber)}");
        builder.AppendLine($"Lieferant;{Escape(order.SupplierName)}");
        builder.AppendLine($"Bestelldatum;{order.OrderDate:dd.MM.yyyy}");
        builder.AppendLine($"Liefertermin;{order.ExpectedDeliveryDate:dd.MM.yyyy}");
        builder.AppendLine($"Status;{order.Status}");
        builder.AppendLine($"Gesamtsumme netto;{order.TotalNet.ToString("0.00", Culture)}");
        builder.AppendLine();
        builder.AppendLine(
            "Artikelnummer;Beschreibung;Bestellmenge;Wareneingang;Offen;Einheit;Einzelpreis netto;Positionssumme netto");

        foreach (var line in order.Lines)
        {
            builder.Append(Escape(line.ArticleNumber)).Append(';')
                .Append(Escape(line.Description)).Append(';')
                .Append(Number(line.OrderedQuantity)).Append(';')
                .Append(Number(line.ReceivedQuantity)).Append(';')
                .Append(Number(line.OpenQuantity)).Append(';')
                .Append(Escape(line.Unit)).Append(';')
                .Append(line.UnitPriceNet.ToString("0.0000", Culture)).Append(';')
                .Append(line.LineTotalNet.ToString("0.00", Culture))
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
