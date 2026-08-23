using System.Globalization;
using System.Text;
using WerkPilot.Application.Billing;

namespace WerkPilot.Infrastructure.Billing;

public sealed class CustomerCreditNoteCsvExporter : ICustomerCreditNoteCsvExporter
{
    private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("de-AT");

    public string Export(CustomerCreditNoteDto note)
    {
        var b = new StringBuilder();
        b.AppendLine($"Gutschriftsnummer;{Escape(note.CreditNoteNumber)}");
        b.AppendLine($"Ausgangsrechnung;{Escape(note.CustomerInvoiceNumber)}");
        b.AppendLine($"Kunde;{Escape(note.CustomerName)}");
        b.AppendLine($"Gutschriftsdatum;{note.CreditNoteDate:dd.MM.yyyy}");
        b.AppendLine($"Status;{note.Status}");
        b.AppendLine($"Grund;{Escape(note.Reason)}");
        b.AppendLine($"Netto;{Money(note.NetTotal)}");
        b.AppendLine($"Umsatzsteuer;{Money(note.VatTotal)}");
        b.AppendLine($"Brutto;{Money(note.GrossTotal)}");
        b.AppendLine();
        b.AppendLine("Beschreibung;Menge;Einheit;Einzelpreis netto;USt Prozent;Netto;USt;Brutto");

        foreach (var line in note.Lines)
        {
            b.Append(Escape(line.Description)).Append(';')
                .Append(line.Quantity.ToString("0.###", Culture)).Append(';')
                .Append(Escape(line.Unit)).Append(';')
                .Append(line.UnitPriceNet.ToString("0.0000", Culture)).Append(';')
                .Append(line.VatRatePercent.ToString("0.00", Culture)).Append(';')
                .Append(Money(line.NetTotal)).Append(';')
                .Append(Money(line.VatAmount)).Append(';')
                .Append(Money(line.GrossTotal)).AppendLine();
        }

        return b.ToString();
    }

    private static string Money(decimal value) => value.ToString("0.00", Culture);
    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Contains(';') || value.Contains('"') || value.Contains('\n')
            ? $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\""
            : value;
    }
}
