using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WerkPilot.Application.Billing;

namespace WerkPilot.Infrastructure.Billing;

public sealed class CustomerInvoicePdfExporter : ICustomerInvoicePdfExporter
{
    private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("de-AT");

    public Task<string> ExportAsync(
        CustomerInvoiceDocumentData document,
        string destinationDirectory,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Directory.CreateDirectory(destinationDirectory);
        var path = Path.Combine(destinationDirectory, $"{Safe(document.Invoice.InvoiceNumber)}.pdf");

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(34);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text(document.Company.CompanyName).FontSize(21).Bold().FontColor(Colors.Teal.Darken2);
                        c.Item().Text("RECHNUNG").FontSize(17).SemiBold();
                        c.Item().Text(document.Invoice.CustomerName).Bold();
                    });

                    row.ConstantItem(230).AlignRight().Column(c =>
                    {
                        c.Item().Text(document.Invoice.InvoiceNumber).Bold();
                        c.Item().Text($"Rechnungsdatum: {document.Invoice.InvoiceDate:dd.MM.yyyy}");
                        c.Item().Text($"Fällig am: {document.Invoice.DueDate:dd.MM.yyyy}");
                    });
                });

                page.Content().PaddingVertical(18).Column(column =>
                {
                    column.Spacing(14);
                    column.Item().Text("Wir verrechnen Ihnen folgende Leistungen:");

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(36);
                            cols.RelativeColumn();
                            cols.ConstantColumn(70);
                            cols.ConstantColumn(75);
                            cols.ConstantColumn(90);
                        });

                        table.Header(header =>
                        {
                            Header(header.Cell(), "Pos.");
                            Header(header.Cell(), "Beschreibung");
                            Header(header.Cell(), "Menge");
                            Header(header.Cell(), "USt");
                            Header(header.Cell(), "Summe netto");
                        });

                        var pos = 1;
                        foreach (var line in document.Invoice.Lines)
                        {
                            Cell(table.Cell(), (pos++).ToString(Culture));
                            Cell(table.Cell(), line.Description);
                            Cell(table.Cell(), $"{line.Quantity:N3} {line.Unit}", true);
                            Cell(table.Cell(), $"{line.VatRatePercent:N2} %", true);
                            Cell(table.Cell(), line.NetTotal.ToString("C2", Culture), true);
                        }
                    });

                    column.Item().AlignRight().Width(280).Column(total =>
                    {
                        Total(total, "Nettosumme", document.Invoice.NetTotal);
                        Total(total, "Umsatzsteuer", document.Invoice.VatTotal);
                        Total(total, "Gesamt brutto", document.Invoice.GrossTotal, true);
                        Total(total, "Bezahlt", document.Invoice.PaidAmount);
                        Total(total, "Gutschriften", document.Invoice.CreditedAmount);
                        Total(total, "Offener Betrag", document.Invoice.OpenAmount, true);
                    });

                    column.Item().Text(
                        $"Bitte überweisen Sie den offenen Betrag bis {document.Invoice.DueDate:dd.MM.yyyy} unter Angabe der Rechnungsnummer.");
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span($"{document.Company.CompanyName} · Rechnung {document.Invoice.InvoiceNumber} · Seite ");
                    text.CurrentPageNumber();
                    text.Span(" von ");
                    text.TotalPages();
                });
            });
        }).GeneratePdf(path);

        return Task.FromResult(path);
    }

    private static void Header(IContainer c, string text) =>
        c.Background(Colors.Teal.Darken2).Padding(6).Text(text).FontColor(Colors.White).SemiBold();

    private static void Cell(IContainer c, string text, bool right = false)
    {
        var cell = c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6);
        if (right) cell.AlignRight().Text(text); else cell.Text(text);
    }

    private static void Total(ColumnDescriptor column, string label, decimal value, bool bold = false)
    {
        column.Item().Row(row =>
        {
            var left = row.RelativeItem().Text(label);
            var right = row.ConstantItem(110).AlignRight().Text(value.ToString("C2", Culture));
            if (bold) { left.Bold(); right.Bold(); }
        });
    }

    private static string Safe(string value) =>
        string.Concat(value.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
}
