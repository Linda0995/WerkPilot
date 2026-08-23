using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WerkPilot.Application.Billing;
using WerkPilot.Application.Settings;
using WerkPilot.Domain.Billing;

namespace WerkPilot.Infrastructure.Billing;

public sealed class DunningNoticePdfExporter : IDunningNoticePdfExporter
{
    private static readonly CultureInfo Culture =
        CultureInfo.GetCultureInfo("de-AT");

    public Task<string> ExportAsync(
        DunningNoticeDto notice,
        CompanyProfileDto company,
        string destinationDirectory,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Directory.CreateDirectory(destinationDirectory);

        var path = Path.Combine(
            destinationDirectory,
            $"{notice.NoticeNumber}.pdf");

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(38);
                page.DefaultTextStyle(x => x.FontSize(10.5f));

                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text(company.CompanyName)
                            .FontSize(21).Bold()
                            .FontColor(Colors.Teal.Darken2);
                        c.Item().Text(Title(notice.Level))
                            .FontSize(17).SemiBold();
                        c.Item().PaddingTop(10).Text(notice.CustomerName).Bold();
                    });

                    row.ConstantItem(230).AlignRight().Column(c =>
                    {
                        c.Item().Text(notice.NoticeNumber).Bold();
                        c.Item().Text($"Mahndatum: {notice.NoticeDate:dd.MM.yyyy}");
                        c.Item().Text($"Rechnung: {notice.CustomerInvoiceNumber}");
                        c.Item().Text($"Überfällig: {notice.OverdueDays} Tage");
                    });
                });

                page.Content().PaddingVertical(24).Column(column =>
                {
                    column.Spacing(14);
                    column.Item().Text(Intro(notice.Level));

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.ConstantColumn(120);
                        });

                        Row(table, "Offener Rechnungsbetrag", notice.PrincipalAmount);
                        Row(table, "Mahngebühr", notice.FeeAmount);
                        Row(
                            table,
                            $"Verzugszinsen ({notice.AnnualInterestRatePercent:N2} % p.a.)",
                            notice.InterestAmount);
                    });

                    column.Item().AlignRight().Width(320)
                        .BorderTop(1.5f)
                        .BorderColor(Colors.Grey.Darken1)
                        .PaddingTop(8)
                        .Row(row =>
                        {
                            row.RelativeItem().Text("Gesamtforderung").Bold().FontSize(13);
                            row.ConstantItem(130).AlignRight()
                                .Text(notice.TotalDue.ToString("C2", Culture))
                                .Bold().FontSize(13);
                        });

                    column.Item().PaddingTop(10).Text(
                        $"Bitte begleichen Sie die Gesamtforderung bis spätestens {notice.PaymentDeadline:dd.MM.yyyy} unter Angabe der Rechnungsnummer {notice.CustomerInvoiceNumber}.");

                    if (notice.Level == DunningLevel.FinalDunning)
                    {
                        column.Item()
                            .Background(Colors.Red.Lighten4)
                            .Padding(12)
                            .Text(
                                "Nach fruchtlosem Ablauf dieser Frist behalten wir uns die Übergabe an ein Inkassobüro oder die Einleitung rechtlicher Schritte vor.")
                            .SemiBold();
                    }
                });

                page.Footer().AlignCenter().Text(
                    $"{company.CompanyName} · {notice.NoticeNumber}");
            });
        }).GeneratePdf(path);

        return Task.FromResult(path);
    }

    private static string Title(DunningLevel level) => level switch
    {
        DunningLevel.Reminder => "ZAHLUNGSERINNERUNG",
        DunningLevel.FirstDunning => "1. MAHNUNG",
        DunningLevel.SecondDunning => "2. MAHNUNG",
        DunningLevel.FinalDunning => "LETZTE MAHNUNG",
        _ => "MAHNUNG"
    };

    private static string Intro(DunningLevel level) => level switch
    {
        DunningLevel.Reminder =>
            "Bei der Durchsicht unserer offenen Posten haben wir festgestellt, dass die nachstehende Rechnung noch nicht vollständig beglichen wurde. Möglicherweise wurde die Zahlung übersehen.",
        DunningLevel.FirstDunning =>
            "Trotz unserer Zahlungserinnerung ist die nachstehende Forderung weiterhin offen.",
        DunningLevel.SecondDunning =>
            "Unsere bisherigen Zahlungshinweise blieben leider ohne vollständigen Zahlungseingang.",
        DunningLevel.FinalDunning =>
            "Wir fordern Sie letztmalig auf, die nachstehende Gesamtforderung innerhalb der gesetzten Frist zu begleichen.",
        _ => "Die nachstehende Forderung ist überfällig."
    };

    private static void Row(
        TableDescriptor table,
        string label,
        decimal value)
    {
        table.Cell().PaddingVertical(5).Text(label);
        table.Cell().PaddingVertical(5).AlignRight()
            .Text(value.ToString("C2", Culture));
    }
}
