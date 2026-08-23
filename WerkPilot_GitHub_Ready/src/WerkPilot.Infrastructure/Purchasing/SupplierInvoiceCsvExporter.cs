using System.Globalization;
using System.Text;
using WerkPilot.Application.Purchasing;

namespace WerkPilot.Infrastructure.Purchasing;

public sealed class SupplierInvoiceCsvExporter : ISupplierInvoiceCsvExporter
{
    private static readonly CultureInfo Culture =
        CultureInfo.GetCultureInfo("de-AT");

    public string Export(SupplierInvoiceDto invoice)
    {
        var builder = new StringBuilder();

        builder.AppendLine($"Rechnungsnummer;{Escape(invoice.InvoiceNumber)}");
        builder.AppendLine($"Bestellnummer;{Escape(invoice.SupplierOrderNumber)}");
        builder.AppendLine($"Lieferant;{Escape(invoice.SupplierName)}");
        builder.AppendLine($"Rechnungsdatum;{invoice.InvoiceDate:dd.MM.yyyy}");
        builder.AppendLine($"Fälligkeit;{invoice.DueDate:dd.MM.yyyy}");
        builder.AppendLine($"Status;{invoice.Status}");
        builder.AppendLine($"Prüfstatus;{invoice.MatchStatus}");
        builder.AppendLine($"Gesamtsumme netto;{invoice.TotalNet.ToString("0.00", Culture)}");
        builder.AppendLine($"Bezahlt;{invoice.PaidAmount.ToString("0.00", Culture)}");
        builder.AppendLine($"Offen;{invoice.OpenAmount.ToString("0.00", Culture)}");
        builder.AppendLine($"Skonto Prozent;{invoice.CashDiscountPercent.ToString("0.00", Culture)}");
        builder.AppendLine($"Skontofrist;{invoice.CashDiscountDueDate:dd.MM.yyyy}");
        builder.AppendLine($"Skontobetrag;{invoice.CashDiscountAmount.ToString("0.00", Culture)}");
        builder.AppendLine($"Gesamtabweichung;{invoice.TotalVariance.ToString("0.00", Culture)}");
        builder.AppendLine($"Warnungen;{invoice.WarningCount}");
        builder.AppendLine($"Kritische Abweichungen;{invoice.CriticalCount}");
        builder.AppendLine();
        builder.AppendLine(
            "Artikelnummer;Beschreibung;Bestellt;Wareneingang;Verrechnet;Bestellpreis;Rechnungspreis;Mengendifferenz;Preisabweichung;Wertabweichung;Prüfstatus");

        foreach (var line in invoice.Lines)
        {
            builder.Append(Escape(line.ArticleNumber)).Append(';')
                .Append(Escape(line.Description)).Append(';')
                .Append(Number(line.OrderedQuantity)).Append(';')
                .Append(Number(line.ReceivedQuantity)).Append(';')
                .Append(Number(line.InvoicedQuantity)).Append(';')
                .Append(line.OrderedUnitPriceNet.ToString("0.0000", Culture)).Append(';')
                .Append(line.InvoicedUnitPriceNet.ToString("0.0000", Culture)).Append(';')
                .Append(Number(line.QuantityVariance)).Append(';')
                .Append(line.PriceVariancePerUnit.ToString("0.0000", Culture)).Append(';')
                .Append(line.ValueVariance.ToString("0.00", Culture)).Append(';')
                .Append(line.MatchStatus)
                .AppendLine();
        }

        builder.AppendLine();
        builder.AppendLine("Zahlungsdatum;Betrag;Referenz;Erfasst von");

        foreach (var payment in invoice.Payments)
        {
            builder.Append(payment.PaymentDate.ToString("dd.MM.yyyy", Culture)).Append(';')
                .Append(payment.Amount.ToString("0.00", Culture)).Append(';')
                .Append(Escape(payment.Reference)).Append(';')
                .Append(Escape(payment.CreatedBy))
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
