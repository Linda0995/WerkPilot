using WerkPilot.Domain.Documents;

namespace WerkPilot.UnitTests;

public sealed class DocumentMoveTests
{
    [Fact]
    public void File_CanMoveBetweenFolderAndRoot()
    {
        var file = new DocumentFile(
            "Plan.pdf",
            "stored.pdf",
            "2026-08/stored.pdf",
            "application/pdf",
            100,
            DocumentOwnerType.Project,
            Guid.NewGuid(),
            null);

        var folderId = Guid.NewGuid();
        file.Move(folderId);
        Assert.Equal(folderId, file.FolderId);

        file.Move(null);
        Assert.Null(file.FolderId);
    }

    [Fact]
    public void Folder_CanMoveToRoot()
    {
        var folder = new DocumentFolder(
            "Montage",
            DocumentOwnerType.Project,
            Guid.NewGuid(),
            Guid.NewGuid());

        folder.Move(null);

        Assert.Null(folder.ParentFolderId);
    }
}
