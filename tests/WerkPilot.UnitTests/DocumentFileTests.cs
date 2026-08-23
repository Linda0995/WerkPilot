using WerkPilot.Domain.Documents;

namespace WerkPilot.UnitTests;

public sealed class DocumentFileTests
{
    [Fact]
    public void Rename_ChangesDisplayNameOnly()
    {
        var file = new DocumentFile(
            "Plan.pdf",
            "abc123.pdf",
            "2026-08/abc123.pdf",
            "application/pdf",
            1024,
            DocumentOwnerType.Project,
            Guid.NewGuid(),
            null);

        file.Rename("Freigabeplan.pdf");

        Assert.Equal("Freigabeplan.pdf", file.DisplayName);
        Assert.Equal("abc123.pdf", file.StoredFileName);
    }

    [Fact]
    public void NegativeSize_IsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new DocumentFile(
                "Plan.pdf",
                "abc123.pdf",
                "2026-08/abc123.pdf",
                "application/pdf",
                -1,
                DocumentOwnerType.Project,
                Guid.NewGuid(),
                null));
    }

    [Fact]
    public void TrashAndRestore_ChangeState()
    {
        var file = new DocumentFile(
            "Plan.pdf",
            "abc123.pdf",
            "2026-08/abc123.pdf",
            "application/pdf",
            1024,
            DocumentOwnerType.Project,
            Guid.NewGuid(),
            null);

        file.MoveToTrash();
        Assert.True(file.IsDeleted);

        file.Restore();
        Assert.False(file.IsDeleted);
    }
}
