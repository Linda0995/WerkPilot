using WerkPilot.Application.Messaging;
using WerkPilot.Domain.Messaging;

namespace WerkPilot.UnitTests;

public sealed class DocumentEmailTemplateTests
{
    [Fact]
    public void Render_ReplacesKnownPlaceholders()
    {
        var result = DocumentEmailTemplateService.Render(
            "Rechnung {{Belegnummer}} für {{Kundenname}}",
            new Dictionary<string, string>
            {
                ["Belegnummer"] = "RE-2026-0001",
                ["Kundenname"] = "Muster GmbH"
            });

        Assert.Equal(
            "Rechnung RE-2026-0001 für Muster GmbH",
            result);
    }

    [Fact]
    public void Template_CanRemoveDefaultFlag()
    {
        var template = new DocumentEmailTemplate(
            DocumentEmailType.CustomerInvoice,
            "Standard",
            "Betreff",
            "Text",
            true);

        template.RemoveDefault();

        Assert.False(template.IsDefault);
    }
}
