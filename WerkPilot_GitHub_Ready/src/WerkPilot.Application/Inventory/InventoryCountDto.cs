using WerkPilot.Domain.Inventory;

namespace WerkPilot.Application.Inventory;

public sealed record InventoryCountDto(
    Guid Id,
    string CountNumber,
    string Title,
    DateOnly CountDate,
    string? StorageLocation,
    string? CreatedBy,
    InventoryCountStatus Status,
    DateTimeOffset? PostedAtUtc,
    string? PostedBy,
    int CountedLineCount,
    int OpenLineCount,
    decimal AbsoluteDifferenceQuantity,
    decimal AbsoluteDifferenceValue,
    int OutdatedPriceCount,
    IReadOnlyList<InventoryCountLineDto> Lines);
