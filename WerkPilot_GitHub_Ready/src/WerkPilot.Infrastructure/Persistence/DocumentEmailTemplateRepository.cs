using Microsoft.EntityFrameworkCore;
using WerkPilot.Application.Messaging;
using WerkPilot.Domain.Messaging;

namespace WerkPilot.Infrastructure.Persistence;

public sealed class DocumentEmailTemplateRepository(WerkPilotDbContext dbContext)
    : IDocumentEmailTemplateRepository
{
    public async Task<IReadOnlyList<DocumentEmailTemplate>> GetAllAsync(
        CancellationToken cancellationToken) =>
        await dbContext.DocumentEmailTemplates
            .AsNoTracking()
            .OrderBy(x => x.DocumentType)
            .ThenByDescending(x => x.IsDefault)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

    public Task<DocumentEmailTemplate?> GetAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        dbContext.DocumentEmailTemplates
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task AddAsync(
        DocumentEmailTemplate template,
        CancellationToken cancellationToken) =>
        dbContext.DocumentEmailTemplates
            .AddAsync(template, cancellationToken)
            .AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
