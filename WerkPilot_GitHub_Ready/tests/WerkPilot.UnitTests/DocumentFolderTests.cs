using WerkPilot.Domain.Documents;

namespace WerkPilot.UnitTests;

public sealed class DocumentFolderTests
{
    [Fact]
    public void Rename_ChangesName()
    {
        var folder = new DocumentFolder(
            "Planung",
            DocumentOwnerType.Project,
            Guid.NewGuid());

        folder.Rename("Konstruktion");

        Assert.Equal("Konstruktion", folder.Name);
    }

    [Fact]
    public void MoveToSelf_IsRejected()
    {
        var folder = new DocumentFolder(
            "Planung",
            DocumentOwnerType.Project,
            Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() =>
            folder.Move(folder.Id));
    }

    [Fact]
    public void TrashAndRestore_ChangeState()
    {
        var folder = new DocumentFolder(
            "Planung",
            DocumentOwnerType.Project,
            Guid.NewGuid());

        folder.MoveToTrash();
        Assert.True(folder.IsDeleted);

        folder.Restore();
        Assert.False(folder.IsDeleted);
    }
}
