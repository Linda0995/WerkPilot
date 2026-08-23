using WerkPilot.Domain.Documents;

namespace WerkPilot.Application.Documents;

public interface IDocumentRepository
{
    Task<IReadOnlyList<DocumentFolder>> GetFoldersAsync(
        DocumentOwnerType ownerType,
        Guid? ownerId,
        bool includeDeleted,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<DocumentFile>> GetFilesAsync(
        DocumentOwnerType ownerType,
        Guid? ownerId,
        bool includeDeleted,
        CancellationToken cancellationToken);

    Task<DocumentFolder?> GetFolderAsync(Guid id, CancellationToken cancellationToken);
    Task<DocumentFile?> GetFileAsync(Guid id, CancellationToken cancellationToken);
    Task AddFolderAsync(DocumentFolder folder, CancellationToken cancellationToken);
    Task AddFileAsync(DocumentFile file, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
