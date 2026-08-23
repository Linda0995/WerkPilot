using System.Globalization;
using System.Text;
using WerkPilot.Application.Billing;

namespace WerkPilot.Infrastructure.Billing;

public sealed class CustomerInvoiceCsvExporter : ICustomerInvoiceCsvExporter
{
    private static readonly CultureInfo Culture =
        CultureInfo.GetCultureInfo("de-AT");

    public string Export(CustomerInvoiceDto invoice)
    {
        var builder = new StringBuilder();

        builder.AppendLine($"Rechnungsnummer;{Escape(invoice.InvoiceNumber)}");
        builder.AppendLine($"Kunde;{Escape(invoice.CustomerName)}");
        builder.AppendLine($"Rechnungsdatum;{invoice.InvoiceDate:dd.MM.yyyy}");
        builder.AppendLine($"Fälligkeit;{invoice.DueDate:dd.MM.yyyy}");
        builder.AppendLine($"Status;{invoice.Status}");
        builder.AppendLine($"Mahnstufe;{invoice.DunningLevel}");
        builder.AppendLine($"Netto;{Money(invoice.NetTotal)}");
        builder.AppendLine($"Umsatzsteuer;{Money(invoice.VatTotal)}");
        builder.AppendLine($"Brutto;{Money(invoice.GrossTotal)}");
        builder.AppendLine($"Bezahlt;{Money(invoice.PaidAmount)}");
        builder.AppendLine($"Gutgeschrieben;{Money(invoice.CreditedAmount)}");
        builder.AppendLine($"Offen;{Money(invoice.OpenAmount)}");
        builder.AppendLine();
        builder.AppendLine(
            "Beschreibung;Menge;Einheit;Einzelpreis netto;USt Prozent;Netto;USt;Brutto");

        foreach (var line in invoice.Lines)
        {
            builder.Append(Escape(line.Description)).Append(';')
                .Append(line.Quantity.ToString("0.###", Culture)).Append(';')
                .Append(Escape(line.Unit)).Append(';')
                .Append(line.UnitPriceNet.ToString("0.0000", Culture)).Append(';')
                .Append(line.VatRatePercent.ToString("0.00", Culture)).Append(';')
                .Append(Money(line.NetTotal)).Append(';')
                .Append(Money(line.VatAmount)).Append(';')
                .Append(Money(line.GrossTotal))
                .AppendLine();
        }

        builder.AppendLine();
        builder.AppendLine("Zahlungsdatum;Betrag;Referenz;Erfasst von");

        foreach (var payment in invoice.Payments)
        {
            builder.Append(payment.PaymentDate.ToString("dd.MM.yyyy", Culture)).Append(';')
                .Append(Money(payment.Amount)).Append(';')
                .Append(Escape(payment.Reference)).Append(';')
                .Append(Escape(payment.CreatedBy))
                .AppendLine();
        }

        return builder.ToString();
    }

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
