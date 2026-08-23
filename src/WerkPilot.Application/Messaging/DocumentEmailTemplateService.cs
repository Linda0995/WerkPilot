using WerkPilot.Domain.Messaging;

namespace WerkPilot.Application.Messaging;

public sealed class DocumentEmailTemplateService(
    IDocumentEmailTemplateRepository repository)
{
    public async Task<IReadOnlyList<DocumentEmailTemplateDto>> GetAllAsync(
        CancellationToken cancellationToken = default) =>
        (await repository.GetAllAsync(cancellationToken))
            .OrderBy(x => x.DocumentType)
            .ThenByDescending(x => x.IsDefault)
            .ThenBy(x => x.Name)
            .Select(Map)
            .ToArray();

    public async Task<DocumentEmailTemplateDto?> GetDefaultAsync(
        DocumentEmailType documentType,
        CancellationToken cancellationToken = default) =>
        (await repository.GetAllAsync(cancellationToken))
            .Where(x => x.DocumentType == documentType)
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.Name)
            .Select(Map)
            .FirstOrDefault();

    public async Task<DocumentEmailTemplateDto> SaveAsync(
        SaveDocumentEmailTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var all = await repository.GetAllAsync(cancellationToken);

        if (request.IsDefault)
        {
            foreach (var other in all.Where(x =>
                         x.DocumentType == request.DocumentType &&
                         x.IsDefault &&
                         x.Id != request.Id))
            {
                var tracked = await repository.GetAsync(other.Id, cancellationToken);
                tracked?.RemoveDefault();
            }
        }

        DocumentEmailTemplate template;

        if (request.Id.HasValue)
        {
            template = await repository.GetAsync(request.Id.Value, cancellationToken)
                ?? throw new InvalidOperationException("E-Mail-Vorlage wurde nicht gefunden.");

            template.Update(
                request.Name,
                request.SubjectTemplate,
                request.BodyTemplate,
                request.IsDefault);
        }
        else
        {
            template = new DocumentEmailTemplate(
                request.DocumentType,
                request.Name,
                request.SubjectTemplate,
                request.BodyTemplate,
                request.IsDefault);

            await repository.AddAsync(template, cancellationToken);
        }

        await repository.SaveChangesAsync(cancellationToken);
        return Map(template);
    }

    public static string Render(
        string template,
        IReadOnlyDictionary<string, string> values)
    {
        var rendered = template;

        foreach (var pair in values)
            rendered = rendered.Replace(
                $"{{{{{pair.Key}}}}}",
                pair.Value,
                StringComparison.OrdinalIgnoreCase);

        return rendered;
    }

    private static DocumentEmailTemplateDto Map(DocumentEmailTemplate x) => new(
        x.Id,
        x.DocumentType,
        x.Name,
        x.SubjectTemplate,
        x.BodyTemplate,
        x.IsDefault);
}
