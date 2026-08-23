using System.Security.Cryptography;
using System.Text.Json;

namespace WerkPilot.Application.Billing;

public sealed class DocumentArchiveService
{
    public async Task<DocumentArchiveResult> ArchiveAsync(
        string pdfPath,
        string documentType,
        string documentNumber,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(pdfPath))
            throw new FileNotFoundException("PDF-Beleg wurde nicht gefunden.", pdfPath);

        await using var stream = File.OpenRead(pdfPath);
        var hash = Convert.ToHexString(
            await SHA256.HashDataAsync(stream, cancellationToken));

        var manifestPath = Path.ChangeExtension(pdfPath, ".manifest.json");
        var manifest = new
        {
            DocumentType = documentType,
            DocumentNumber = documentNumber,
            PdfFile = Path.GetFileName(pdfPath),
            Sha256 = hash,
            ArchivedAtUtc = DateTimeOffset.UtcNow
        };

        await File.WriteAllTextAsync(
            manifestPath,
            JsonSerializer.Serialize(
                manifest,
                new JsonSerializerOptions { WriteIndented = true }),
            cancellationToken);

        return new DocumentArchiveResult(pdfPath, hash, manifestPath);
    }
}
