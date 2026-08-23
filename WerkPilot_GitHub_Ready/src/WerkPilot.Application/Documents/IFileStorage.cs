namespace WerkPilot.Application.Documents;

public interface IFileStorage
{
    Task<StoredFileResult> StoreAsync(
        string sourcePath,
        string preferredDisplayName,
        CancellationToken cancellationToken = default);

    string GetAbsolutePath(string relativePath);
}

public sealed record StoredFileResult(
    string StoredFileName,
    string RelativePath,
    string ContentType,
    long SizeBytes);
