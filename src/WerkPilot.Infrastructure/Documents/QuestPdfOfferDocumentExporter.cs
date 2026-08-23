using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WerkPilot.Application.Offers;

namespace WerkPilot.Infrastructure.Documents;

public sealed class QuestPdfOfferDocumentExporter : IOfferDocumentExporter
{
    private static readonly CultureInfo GermanCulture = CultureInfo.GetCultureInfo("de-AT");

    public Task<string> ExportPdfAsync(
        OfferDocumentData document,
        string destinationDirectory,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(document);

        Directory.CreateDirectory(destinationDirectory);

        var safeNumber = string.Concat(
            document.Offer.OfferNumber.Select(character =>
                Path.GetInvalidFileNameChars().Contains(character) ? '_' : character));

        var outputPath = Path.Combine(destinationDirectory, $"{safeNumber}.pdf");

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(35);
                page.DefaultTextStyle(style => style.FontSize(10));

                page.Header().Element(header => ComposeHeader(header, document));
                page.Content().PaddingVertical(20).Element(content => ComposeContent(content, document));
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("WerkPilot · Angebot ");
                    text.Span(document.Offer.OfferNumber);
                    text.Span(" · Seite ");
                    text.CurrentPageNumber();
                    text.Span(" von ");
                    text.TotalPages();
                });
            });
        }).GeneratePdf(outputPath);

        return Task.FromResult(outputPath);
    }

    private static void ComposeHeader(IContainer container, OfferDocumentData document)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text(document.Company.CompanyName).FontSize(22).Bold().FontColor(Colors.Teal.Darken2);
                column.Item().Text("Angebot").FontSize(17).SemiBold();
            });

            row.ConstantItem(220).AlignRight().Column(column =>
            {
                column.Item().Text(document.Offer.OfferNumber).Bold();
                column.Item().Text($"Angebotsdatum: {document.Offer.OfferDate:dd.MM.yyyy}");
                column.Item().Text($"Gültig bis: {document.Offer.ValidUntil:dd.MM.yyyy}");
                column.Item().Text($"Status: {document.Offer.Status}");
            });
        });
    }

    private static void ComposeContent(IContainer container, OfferDocumentData document)
    {
        container.Column(column =>
        {
            column.Spacing(14);

            column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(12).Column(address =>
            {
                address.Item().Text(document.CustomerName).Bold();
                if (!string.IsNullOrWhiteSpace(document.ContactPerson))
                    address.Item().Text($"z. H. {document.ContactPerson}");
                if (!string.IsNullOrWhiteSpace(document.Street))
                    address.Item().Text(document.Street);
                address.Item().Text($"{document.PostalCode} {document.City}".Trim());
                address.Item().Text(document.CountryCode);
                address.Item().Text($"Kundennummer: {document.CustomerNumber}");
                if (!string.IsNullOrWhiteSpace(document.VatId))
                    address.Item().Text($"UID/ATU: {document.VatId}");
            });

            column.Item().Text(document.Offer.Title).FontSize(16).Bold();
            column.Item().Text(document.Company.OfferIntroText);

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(38);
                    columns.RelativeColumn();
                    columns.ConstantColumn(65);
                    columns.ConstantColumn(90);
                    columns.ConstantColumn(90);
                });

                table.Header(header =>
                {
                    HeaderCell(header.Cell(), "Pos.");
                    HeaderCell(header.Cell(), "Beschreibung");
                    HeaderCell(header.Cell(), "Menge");
                    HeaderCell(header.Cell(), "Einzel netto");
                    HeaderCell(header.Cell(), "Summe netto");
                });

                foreach (var position in document.Offer.Positions.OrderBy(x => x.PositionNumber))
                {
                    BodyCell(table.Cell(), position.PositionNumber.ToString(GermanCulture));
                    BodyCell(table.Cell(), position.IsOptional ? $"{position.Description} (Alternativposition)" : position.Description);
                    BodyCell(table.Cell(), position.Quantity.ToString("N3", GermanCulture), alignRight: true);
                    BodyCell(table.Cell(), position.UnitPriceNet.ToString("C2", GermanCulture), alignRight: true);
                    BodyCell(table.Cell(), position.TotalNet.ToString("C2", GermanCulture), alignRight: true);
                }
            });

            column.Item().AlignRight().Width(250).Column(total =>
            {
                TotalRow(total, "Nettosumme", document.Offer.NetTotal);
                TotalRow(total, $"Umsatzsteuer {document.Offer.TaxRate:N2} %", document.Offer.TaxTotal);
                total.Item().PaddingTop(5).BorderTop(1).BorderColor(Colors.Grey.Medium);
                TotalRow(total, "Gesamtsumme brutto", document.Offer.GrossTotal, bold: true);
            });

            column.Item().PaddingTop(12).Text(document.Company.OfferClosingText);

            column.Item().PaddingTop(18).BorderTop(1).BorderColor(Colors.Grey.Lighten2)
                .PaddingTop(8).Text(text =>
                {
                    text.Span(document.Company.CompanyName).SemiBold();
                    var address = string.Join(", ", new[]
                    {
                        document.Company.Street,
                        $"{document.Company.PostalCode} {document.Company.City}".Trim()
                    }.Where(value => !string.IsNullOrWhiteSpace(value)));
                    if (!string.IsNullOrWhiteSpace(address))
                        text.Span($" · {address}");
                    if (!string.IsNullOrWhiteSpace(document.Company.Email))
                        text.Span($" · {document.Company.Email}");
                    if (!string.IsNullOrWhiteSpace(document.Company.Phone))
                        text.Span($" · {document.Company.Phone}");
                    if (!string.IsNullOrWhiteSpace(document.Company.VatId))
                        text.Span($" · UID: {document.Company.VatId}");
                });
        });
    }

    private static void HeaderCell(IContainer container, string text) =>
        container
            .Background(Colors.Teal.Darken2)
            .PaddingVertical(7)
            .PaddingHorizontal(5)
            .Text(text)
            .FontColor(Colors.White)
            .SemiBold();

    private static void BodyCell(IContainer container, string text, bool alignRight = false)
    {
        var cell = container
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(6)
            .PaddingHorizontal(5);

        if (alignRight)
            cell.AlignRight().Text(text);
        else
            cell.Text(text);
    }

    private static void TotalRow(ColumnDescriptor column, string label, decimal value, bool bold = false)
    {
        column.Item().Row(row =>
        {
            var labelText = row.RelativeItem().Text(label);
            var valueText = row.ConstantItem(100).AlignRight().Text(value.ToString("C2", GermanCulture));

            if (bold)
            {
                labelText.Bold();
                valueText.Bold();
            }
        });
    }
}
