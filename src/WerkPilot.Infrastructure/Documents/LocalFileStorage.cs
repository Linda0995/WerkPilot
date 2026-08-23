using WerkPilot.Application.Documents;

namespace WerkPilot.Infrastructure.Documents;

public sealed class LocalFileStorage : IFileStorage
{
    private readonly string _rootDirectory;

    public LocalFileStorage()
    {
        _rootDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "WerkPilot",
            "Dateiablage");
    }

    public async Task<StoredFileResult> StoreAsync(
        string sourcePath,
        string preferredDisplayName,
        CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_rootDirectory);

        var extension = Path.GetExtension(sourcePath);
        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var yearMonth = DateTime.Now.ToString("yyyy-MM");
        var relativeDirectory = yearMonth;
        var targetDirectory = Path.Combine(_rootDirectory, relativeDirectory);
        Directory.CreateDirectory(targetDirectory);

        var targetPath = Path.Combine(targetDirectory, storedFileName);

        await using (var source = File.OpenRead(sourcePath))
        await using (var target = File.Create(targetPath))
        {
            await source.CopyToAsync(target, cancellationToken);
        }

        var info = new FileInfo(targetPath);

        return new StoredFileResult(
            storedFileName,
            Path.Combine(relativeDirectory, storedFileName),
            ResolveContentType(extension),
            info.Length);
    }

    public string GetAbsolutePath(string relativePath) =>
        Path.GetFullPath(Path.Combine(_rootDirectory, relativePath));

    private static string ResolveContentType(string extension) =>
        extension.ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".csv" => "text/csv",
            ".txt" => "text/plain",
            ".dxf" => "application/dxf",
            ".dwg" => "application/acad",
            _ => "application/octet-stream"
        };
}
