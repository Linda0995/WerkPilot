using WerkPilot.Domain.Documents;

namespace WerkPilot.Application.Documents;

public sealed record ImportDocumentFileRequest(
    string SourcePath,
    string DisplayName,
    DocumentOwnerType OwnerType,
    Guid? OwnerId,
    Guid? FolderId);
