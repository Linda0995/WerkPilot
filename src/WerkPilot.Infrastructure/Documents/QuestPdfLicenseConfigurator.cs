using QuestPDF.Infrastructure;

namespace WerkPilot.Infrastructure.Documents;

public static class QuestPdfLicenseConfigurator
{
    public static void Configure()
    {
        var configuredLicense = Environment
            .GetEnvironmentVariable("WERKPILOT_QUESTPDF_LICENSE")
            ?.Trim()
            .ToLowerInvariant();

        QuestPDF.Settings.License = configuredLicense switch
        {
            "professional" => LicenseType.Professional,
            "enterprise" => LicenseType.Enterprise,
            _ => LicenseType.Community
        };
    }
}
