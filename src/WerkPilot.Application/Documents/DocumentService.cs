using WerkPilot.Application.Auditing;
using WerkPilot.Domain.Documents;

namespace WerkPilot.Application.Documents;

public sealed class DocumentService(
    IDocumentRepository repository,
    IFileStorage fileStorage,
    IAuditTrail auditTrail)
{
    public async Task<IReadOnlyList<DocumentFolderDto>> GetFoldersAsync(
        DocumentOwnerType ownerType,
        Guid? ownerId,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default) =>
        (await repository.GetFoldersAsync(ownerType, ownerId, includeDeleted, cancellationToken))
            .Select(Map)
            .ToArray();

    public async Task<IReadOnlyList<DocumentFileDto>> GetFilesAsync(
        DocumentOwnerType ownerType,
        Guid? ownerId,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default) =>
        (await repository.GetFilesAsync(ownerType, ownerId, includeDeleted, cancellationToken))
            .Select(Map)
            .ToArray();

    public async Task<DocumentFolderDto> CreateFolderAsync(
        string name,
        DocumentOwnerType ownerType,
        Guid? ownerId,
        Guid? parentFolderId,
        CancellationToken cancellationToken = default)
    {
        var folder = new DocumentFolder(name, ownerType, ownerId, parentFolderId);
        await repository.AddFolderAsync(folder, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        await auditTrail.WriteAsync(
            "DocumentFolder",
            folder.Id,
            "Created",
            $"Ordner „{folder.Name}“ wurde angelegt.",
            cancellationToken);

        return Map(folder);
    }

    public async Task<IReadOnlyList<DocumentFileDto>> ImportFilesAsync(
        IReadOnlyList<ImportDocumentFileRequest> requests,
        CancellationToken cancellationToken = default)
    {
        if (requests.Count == 0)
            return [];

        var imported = new List<DocumentFileDto>();

        foreach (var request in requests)
        {
            imported.Add(await ImportFileAsync(
                request.SourcePath,
                request.DisplayName,
                request.OwnerType,
                request.OwnerId,
                request.FolderId,
                cancellationToken));
        }

        return imported;
    }

    public async Task<DocumentFileDto> ImportFileAsync(
        string sourcePath,
        string displayName,
        DocumentOwnerType ownerType,
        Guid? ownerId,
        Guid? folderId,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(sourcePath))
            throw new FileNotFoundException("Die zu importierende Datei wurde nicht gefunden.", sourcePath);

        var stored = await fileStorage.StoreAsync(sourcePath, displayName, cancellationToken);
        var file = new DocumentFile(
            displayName,
            stored.StoredFileName,
            stored.RelativePath,
            stored.ContentType,
            stored.SizeBytes,
            ownerType,
            ownerId,
            folderId);

        await repository.AddFileAsync(file, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        await auditTrail.WriteAsync(
            "DocumentFile",
            file.Id,
            "Imported",
            $"Datei „{file.DisplayName}“ wurde importiert.",
            cancellationToken);

        return Map(file);
    }

    public async Task RenameFolderAsync(
        Guid id,
        string name,
        CancellationToken cancellationToken = default)
    {
        var folder = await GetRequiredFolderAsync(id, cancellationToken);
        folder.Rename(name);
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task RenameFileAsync(
        Guid id,
        string displayName,
        CancellationToken cancellationToken = default)
    {
        var file = await GetRequiredFileAsync(id, cancellationToken);
        file.Rename(displayName);
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task MoveFileAsync(
        Guid fileId,
        Guid? folderId,
        CancellationToken cancellationToken = default)
    {
        var file = await GetRequiredFileAsync(fileId, cancellationToken);

        if (folderId.HasValue)
        {
            var targetFolder = await GetRequiredFolderAsync(folderId.Value, cancellationToken);
            if (targetFolder.IsDeleted)
                throw new InvalidOperationException("In einen Ordner im Papierkorb kann nicht verschoben werden.");

            if (targetFolder.OwnerType != file.OwnerType || targetFolder.OwnerId != file.OwnerId)
                throw new InvalidOperationException("Datei und Zielordner gehören nicht zur selben Akte.");
        }

        file.Move(folderId);
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task MoveFolderAsync(
        Guid folderId,
        Guid? parentFolderId,
        CancellationToken cancellationToken = default)
    {
        var folder = await GetRequiredFolderAsync(folderId, cancellationToken);

        if (parentFolderId.HasValue)
        {
            var target = await GetRequiredFolderAsync(parentFolderId.Value, cancellationToken);

            if (target.IsDeleted)
                throw new InvalidOperationException("In einen Ordner im Papierkorb kann nicht verschoben werden.");

            if (target.OwnerType != folder.OwnerType || target.OwnerId != folder.OwnerId)
                throw new InvalidOperationException("Ordner und Zielordner gehören nicht zur selben Akte.");

            await EnsureNoFolderCycleAsync(folder.Id, target.Id, cancellationToken);
        }

        folder.Move(parentFolderId);
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task MoveFileToTrashAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var file = await GetRequiredFileAsync(id, cancellationToken);
        file.MoveToTrash();
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task RestoreFileAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var file = await GetRequiredFileAsync(id, cancellationToken);
        file.Restore();
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task MoveFolderToTrashAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var folder = await GetRequiredFolderAsync(id, cancellationToken);
        folder.MoveToTrash();
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task RestoreFolderAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var folder = await GetRequiredFolderAsync(id, cancellationToken);
        folder.Restore();
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<string> GetAbsolutePathAsync(
        Guid fileId,
        CancellationToken cancellationToken = default)
    {
        var file = await GetRequiredFileAsync(fileId, cancellationToken);
        return fileStorage.GetAbsolutePath(file.RelativePath);
    }

    private async Task EnsureNoFolderCycleAsync(
        Guid folderId,
        Guid targetFolderId,
        CancellationToken cancellationToken)
    {
        var currentId = (Guid?)targetFolderId;

        while (currentId.HasValue)
        {
            if (currentId.Value == folderId)
                throw new InvalidOperationException(
                    "Ein Ordner kann nicht in einen eigenen Unterordner verschoben werden.");

            var current = await repository.GetFolderAsync(currentId.Value, cancellationToken);
            currentId = current?.ParentFolderId;
        }
    }

    private async Task<DocumentFolder> GetRequiredFolderAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        await repository.GetFolderAsync(id, cancellationToken)
        ?? throw new InvalidOperationException("Ordner wurde nicht gefunden.");

    private async Task<DocumentFile> GetRequiredFileAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        await repository.GetFileAsync(id, cancellationToken)
        ?? throw new InvalidOperationException("Datei wurde nicht gefunden.");

    private static DocumentFolderDto Map(DocumentFolder x) => new(
        x.Id,
        x.Name,
        x.OwnerType,
        x.OwnerId,
        x.ParentFolderId,
        x.IsDeleted);

    private static DocumentFileDto Map(DocumentFile x) => new(
        x.Id,
        x.DisplayName,
        x.StoredFileName,
        x.RelativePath,
        x.ContentType,
        x.SizeBytes,
        x.OwnerType,
        x.OwnerId,
        x.FolderId,
        x.UploadedAtUtc,
        x.IsDeleted);
}
