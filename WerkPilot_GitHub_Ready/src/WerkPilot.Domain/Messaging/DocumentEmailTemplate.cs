using WerkPilot.Domain.Common;

namespace WerkPilot.Domain.Messaging;

public sealed class DocumentEmailTemplate : Entity
{
    private DocumentEmailTemplate() { }

    public DocumentEmailTemplate(
        DocumentEmailType documentType,
        string name,
        string subjectTemplate,
        string bodyTemplate,
        bool isDefault)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Vorlagenname erforderlich.", nameof(name));
        if (string.IsNullOrWhiteSpace(subjectTemplate))
            throw new ArgumentException("Betreffvorlage erforderlich.", nameof(subjectTemplate));
        if (string.IsNullOrWhiteSpace(bodyTemplate))
            throw new ArgumentException("Textvorlage erforderlich.", nameof(bodyTemplate));

        DocumentType = documentType;
        Name = name.Trim();
        SubjectTemplate = subjectTemplate.Trim();
        BodyTemplate = bodyTemplate.Trim();
        IsDefault = isDefault;
    }

    public DocumentEmailType DocumentType { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string SubjectTemplate { get; private set; } = string.Empty;
    public string BodyTemplate { get; private set; } = string.Empty;
    public bool IsDefault { get; private set; }

    public void Update(
        string name,
        string subjectTemplate,
        string bodyTemplate,
        bool isDefault)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Vorlagenname erforderlich.", nameof(name));
        if (string.IsNullOrWhiteSpace(subjectTemplate))
            throw new ArgumentException("Betreffvorlage erforderlich.", nameof(subjectTemplate));
        if (string.IsNullOrWhiteSpace(bodyTemplate))
            throw new ArgumentException("Textvorlage erforderlich.", nameof(bodyTemplate));

        Name = name.Trim();
        SubjectTemplate = subjectTemplate.Trim();
        BodyTemplate = bodyTemplate.Trim();
        IsDefault = isDefault;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void RemoveDefault()
    {
        IsDefault = false;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
