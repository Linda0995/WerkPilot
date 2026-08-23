using WerkPilot.Application.Settings;

namespace WerkPilot.Application.Billing;

public interface IDunningNoticePdfExporter
{
    Task<string> ExportAsync(
        DunningNoticeDto notice,
        CompanyProfileDto company,
        string destinationDirectory,
        CancellationToken cancellationToken = default);
}
