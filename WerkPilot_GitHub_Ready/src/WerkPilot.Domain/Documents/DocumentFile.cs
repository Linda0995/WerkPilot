using WerkPilot.Domain.Common;

namespace WerkPilot.Domain.Documents;

public sealed class DocumentFile : Entity
{
    private DocumentFile() { }

    public DocumentFile(
        string displayName,
        string storedFileName,
        string relativePath,
        string contentType,
        long sizeBytes,
        DocumentOwnerType ownerType,
        Guid? ownerId,
        Guid? folderId)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Dateiname erforderlich.", nameof(displayName));
        if (string.IsNullOrWhiteSpace(storedFileName))
            throw new ArgumentException("Gespeicherter Dateiname erforderlich.", nameof(storedFileName));
        if (string.IsNullOrWhiteSpace(relativePath))
            throw new ArgumentException("Relativer Pfad erforderlich.", nameof(relativePath));
        if (sizeBytes < 0)
            throw new ArgumentOutOfRangeException(nameof(sizeBytes));

        DisplayName = displayName.Trim();
        StoredFileName = storedFileName.Trim();
        RelativePath = relativePath.Trim();
        ContentType = string.IsNullOrWhiteSpace(contentType)
            ? "application/octet-stream"
            : contentType.Trim();
        SizeBytes = sizeBytes;
        OwnerType = ownerType;
        OwnerId = ownerId;
        FolderId = folderId;
        UploadedAtUtc = DateTimeOffset.UtcNow;
    }

    public string DisplayName { get; private set; } = string.Empty;
    public string StoredFileName { get; private set; } = string.Empty;
    public string RelativePath { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long SizeBytes { get; private set; }
    public DocumentOwnerType OwnerType { get; private set; }
    public Guid? OwnerId { get; private set; }
    public Guid? FolderId { get; private set; }
    public DateTimeOffset UploadedAtUtc { get; private set; }

    public void Rename(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Dateiname erforderlich.", nameof(displayName));

        DisplayName = displayName.Trim();
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Move(Guid? folderId)
    {
        FolderId = folderId;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public new void MoveToTrash()
    {
        IsDeleted = true;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public new void Restore()
    {
        IsDeleted = false;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
