namespace WerkPilot.Application.ProjectCosts;

public interface IProjectClosingReportExporter
{
    string ExportCsv(ProjectClosingReportDto report);
    string ExportMarkdown(ProjectClosingReportDto report);
}
