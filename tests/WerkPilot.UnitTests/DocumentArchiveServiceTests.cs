using WerkPilot.Application.Billing;

namespace WerkPilot.UnitTests;

public sealed class DocumentArchiveServiceTests
{
    [Fact]
    public async Task Archive_CreatesHashAndManifest()
    {
        var directory = Path.Combine(
            Path.GetTempPath(),
            $"WerkPilot_{Guid.NewGuid():N}");

        Directory.CreateDirectory(directory);
        var pdf = Path.Combine(directory, "RE-2026-0001.pdf");
        await File.WriteAllBytesAsync(pdf, [1, 2, 3, 4]);

        var result = await new DocumentArchiveService().ArchiveAsync(
            pdf,
            "CustomerInvoice",
            "RE-2026-0001");

        Assert.Equal(64, result.Sha256.Length);
        Assert.True(File.Exists(result.ManifestPath));

        Directory.Delete(directory, recursive: true);
    }
}
