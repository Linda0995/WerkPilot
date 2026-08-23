using WerkPilot.Application.Messaging;
using WerkPilot.Domain.Messaging;

namespace WerkPilot.UnitTests;

public sealed class DocumentEmailPreviewTests
{
    [Fact]
    public void Preview_PreservesEditableMailData()
    {
        var preview = new DocumentEmailPreview(
            DocumentEmailType.CustomerInvoice,
            Guid.NewGuid(),
            "RE-2026-0001",
            "Muster GmbH",
            "kunde@example.com",
            "Rechnung",
            "Nachricht",
            "RE-2026-0001.pdf");

        Assert.Equal("kunde@example.com", preview.Recipient);
        Assert.Equal("RE-2026-0001.pdf", preview.AttachmentFileName);
    }
}
