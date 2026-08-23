using System.Globalization;
using System.Text;
using WerkPilot.Application.ProjectCosts;

namespace WerkPilot.Infrastructure.ProjectCosts;

public sealed class ProjectClosingReportExporter : IProjectClosingReportExporter
{
    private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("de-AT");

    public string ExportCsv(ProjectClosingReportDto report)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Bereich;Kennzahl;Wert");
        Add(builder, "Projekt", "Projektnummer", report.ProjectNumber);
        Add(builder, "Projekt", "Titel", report.ProjectTitle);
        Add(builder, "Projekt", "Kunde", report.CustomerName);
        Add(builder, "Projekt", "Projektleitung", report.ProjectManager);
        Add(builder, "Projekt", "Status", report.ProjectStatus.ToString());
        Add(builder, "Projekt", "Fortschritt", $"{report.ProgressPercent} %");
        Add(builder, "Projekt", "Offene Aufgaben", report.OpenTaskCount.ToString(Culture));
        Add(builder, "Projekt", "Abschlussbereit", report.CanBeClosed ? "Ja" : "Nein");
        Add(builder, "Projekt", "Beurteilung", report.ClosingAssessment);

        AddMoney(builder, "Kosten", "Soll-Gesamtkosten", report.CostControlling.PlannedTotalCost);
        AddMoney(builder, "Kosten", "Ist-Gesamtkosten", report.CostControlling.ActualTotalCost);
        AddMoney(builder, "Kosten", "Kostenabweichung", report.CostControlling.VarianceAmount);
        Add(builder, "Kosten", "Budgetverbrauch", $"{report.CostControlling.UtilizationPercent:N1} %");

        Add(builder, "Zeit", "Soll-Stunden", $"{report.PlannedLaborHours:N2} h");
        Add(builder, "Zeit", "Ist-Stunden", $"{report.ActualLaborHours:N2} h");
        Add(builder, "Zeit", "Stundenabweichung", $"{report.LaborVarianceHours:+0.00;-0.00;0.00} h");
        Add(builder, "Zeit", "Zeitbudgetverbrauch", $"{report.LaborUtilizationPercent:N1} %");

        AddMoney(builder, "Ergebnis", "Netto-Verkaufspreis", report.Profitability.RevenueNet);
        AddMoney(builder, "Ergebnis", "Geplanter Deckungsbeitrag", report.Profitability.PlannedContributionMargin);
        AddMoney(builder, "Ergebnis", "Aktueller Deckungsbeitrag", report.Profitability.ActualContributionMargin);
        Add(builder, "Ergebnis", "Aktuelle Marge", $"{report.Profitability.ActualMarginPercent:N1} %");
        AddMoney(builder, "Ergebnis", "Ergebnisabweichung", report.Profitability.ResultVariance);
        Add(builder, "Ergebnis", "Profitabilitätsstatus", report.Profitability.Status.ToString());

        return builder.ToString();
    }

    public string ExportMarkdown(ProjectClosingReportDto report)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"# Projektabschlussbericht {report.ProjectNumber}");
        builder.AppendLine();
        builder.AppendLine($"**Projekt:** {report.ProjectTitle}");
        builder.AppendLine($"**Kunde:** {report.CustomerName}");
        builder.AppendLine($"**Projektleitung:** {report.ProjectManager ?? "–"}");
        builder.AppendLine($"**Erstellt:** {report.GeneratedAtUtc.ToLocalTime():dd.MM.yyyy HH:mm}");
        builder.AppendLine();
        builder.AppendLine("## Abschlussbeurteilung");
        builder.AppendLine();
        builder.AppendLine(report.ClosingAssessment);
        builder.AppendLine();
        builder.AppendLine("## Projektstatus");
        builder.AppendLine();
        builder.AppendLine($"- Status: {report.ProjectStatus}");
        builder.AppendLine($"- Fortschritt: {report.ProgressPercent} %");
        builder.AppendLine($"- Offene Aufgaben: {report.OpenTaskCount}");
        builder.AppendLine($"- Abschlussbereit: {(report.CanBeClosed ? "Ja" : "Nein")}");
        builder.AppendLine();
        builder.AppendLine("## Zeitcontrolling");
        builder.AppendLine();
        builder.AppendLine($"- Soll-Stunden: {report.PlannedLaborHours:N2} h");
        builder.AppendLine($"- Ist-Stunden: {report.ActualLaborHours:N2} h");
        builder.AppendLine($"- Abweichung: {report.LaborVarianceHours:+0.00;-0.00;0.00} h");
        builder.AppendLine($"- Verbrauch: {report.LaborUtilizationPercent:N1} %");
        builder.AppendLine();
        builder.AppendLine("## Kostencontrolling");
        builder.AppendLine();
        builder.AppendLine($"- Soll-Kosten: {report.CostControlling.PlannedTotalCost:N2} €");
        builder.AppendLine($"- Ist-Kosten: {report.CostControlling.ActualTotalCost:N2} €");
        builder.AppendLine($"- Abweichung: {report.CostControlling.VarianceAmount:+0.00;-0.00;0.00} €");
        builder.AppendLine();
        builder.AppendLine("## Ergebnis");
        builder.AppendLine();
        builder.AppendLine($"- Netto-Verkaufspreis: {report.Profitability.RevenueNet:N2} €");
        builder.AppendLine($"- Aktueller Deckungsbeitrag: {report.Profitability.ActualContributionMargin:N2} €");
        builder.AppendLine($"- Aktuelle Marge: {report.Profitability.ActualMarginPercent:N1} %");
        builder.AppendLine($"- Ergebnisabweichung: {report.Profitability.ResultVariance:+0.00;-0.00;0.00} €");
        return builder.ToString();
    }

    private static void AddMoney(
        StringBuilder builder,
        string area,
        string key,
        decimal value) =>
        Add(builder, area, key, $"{value:N2} €");

    private static void Add(
        StringBuilder builder,
        string area,
        string key,
        string? value) =>
        builder.Append(Escape(area)).Append(';')
            .Append(Escape(key)).Append(';')
            .Append(Escape(value))
            .AppendLine();

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value.Contains(';') || value.Contains('"') || value.Contains('\n')
            ? $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\""
            : value;
    }
}
