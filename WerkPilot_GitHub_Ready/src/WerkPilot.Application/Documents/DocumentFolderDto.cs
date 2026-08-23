using WerkPilot.Domain.Documents;

namespace WerkPilot.Application.Documents;

public sealed record DocumentFolderDto(
    Guid Id,
    string Name,
    DocumentOwnerType OwnerType,
    Guid? OwnerId,
    Guid? ParentFolderId,
    bool IsDeleted);
