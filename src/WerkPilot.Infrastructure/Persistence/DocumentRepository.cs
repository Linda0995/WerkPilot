using Microsoft.EntityFrameworkCore;
using WerkPilot.Application.Documents;
using WerkPilot.Domain.Documents;

namespace WerkPilot.Infrastructure.Persistence;

public sealed class DocumentRepository(WerkPilotDbContext dbContext)
    : IDocumentRepository
{
    public async Task<IReadOnlyList<DocumentFolder>> GetFoldersAsync(
        DocumentOwnerType ownerType,
        Guid? ownerId,
        bool includeDeleted,
        CancellationToken cancellationToken)
    {
        IQueryable<DocumentFolder> query = includeDeleted
            ? dbContext.DocumentFolders.IgnoreQueryFilters()
            : dbContext.DocumentFolders;

        return await query
            .Where(x => x.OwnerType == ownerType && x.OwnerId == ownerId)
            .OrderBy(x => x.Name)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DocumentFile>> GetFilesAsync(
        DocumentOwnerType ownerType,
        Guid? ownerId,
        bool includeDeleted,
        CancellationToken cancellationToken)
    {
        IQueryable<DocumentFile> query = includeDeleted
            ? dbContext.DocumentFiles.IgnoreQueryFilters()
            : dbContext.DocumentFiles;

        return await query
            .Where(x => x.OwnerType == ownerType && x.OwnerId == ownerId)
            .OrderByDescending(x => x.UploadedAtUtc)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<DocumentFolder?> GetFolderAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        dbContext.DocumentFolders
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<DocumentFile?> GetFileAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        dbContext.DocumentFiles
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task AddFolderAsync(
        DocumentFolder folder,
        CancellationToken cancellationToken) =>
        dbContext.DocumentFolders.AddAsync(folder, cancellationToken).AsTask();

    public Task AddFileAsync(
        DocumentFile file,
        CancellationToken cancellationToken) =>
        dbContext.DocumentFiles.AddAsync(file, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
