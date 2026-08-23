using WerkPilot.Domain.Common;

namespace WerkPilot.Domain.Documents;

public sealed class DocumentFolder : Entity
{
    private DocumentFolder() { }

    public DocumentFolder(
        string name,
        DocumentOwnerType ownerType,
        Guid? ownerId,
        Guid? parentFolderId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Ordnername erforderlich.", nameof(name));

        Name = name.Trim();
        OwnerType = ownerType;
        OwnerId = ownerId;
        ParentFolderId = parentFolderId;
    }

    public string Name { get; private set; } = string.Empty;
    public DocumentOwnerType OwnerType { get; private set; }
    public Guid? OwnerId { get; private set; }
    public Guid? ParentFolderId { get; private set; }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Ordnername erforderlich.", nameof(name));

        Name = name.Trim();
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Move(Guid? parentFolderId)
    {
        if (parentFolderId == Id)
            throw new InvalidOperationException("Ein Ordner kann nicht in sich selbst verschoben werden.");

        ParentFolderId = parentFolderId;
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
