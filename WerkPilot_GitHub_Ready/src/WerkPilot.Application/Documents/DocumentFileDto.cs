using WerkPilot.Domain.Documents;

namespace WerkPilot.Application.Documents;

public sealed record DocumentFileDto(
    Guid Id,
    string DisplayName,
    string StoredFileName,
    string RelativePath,
    string ContentType,
    long SizeBytes,
    DocumentOwnerType OwnerType,
    Guid? OwnerId,
    Guid? FolderId,
    DateTimeOffset UploadedAtUtc,
    bool IsDeleted);
