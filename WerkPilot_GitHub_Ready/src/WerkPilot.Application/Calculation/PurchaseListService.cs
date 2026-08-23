using WerkPilot.Application.Materials;

namespace WerkPilot.Application.Calculation;

public sealed class PurchaseListService(
    ICalculationRepository calculationRepository,
    IMaterialRepository materialRepository)
{
    public async Task<IReadOnlyList<PurchaseListItemDto>> CreateAsync(
        Guid offerId,
        CancellationToken cancellationToken = default)
    {
        var calculation = await calculationRepository.GetByOfferIdAsync(
            offerId,
            cancellationToken);

        if (calculation is null)
            return [];

        var groups = calculation.Items
            .Where(x => x.MaterialItemId.HasValue)
            .GroupBy(x => x.MaterialItemId!.Value);

        var result = new List<PurchaseListItemDto>();

        foreach (var group in groups)
        {
            var material = await materialRepository.GetAsync(group.Key, cancellationToken);
            if (material is null)
                continue;

            var quantity = group.Sum(x => x.Quantity);
            var currentPrice = material.PurchasePrice;

            result.Add(new PurchaseListItemDto(
                material.Id,
                material.ArticleNumber,
                material.Description,
                material.Unit,
                quantity,
                currentPrice,
                decimal.Round(quantity * currentPrice, 2, MidpointRounding.AwayFromZero),
                material.Supplier,
                DateTimeOffset.UtcNow - material.PriceUpdatedAtUtc > TimeSpan.FromDays(90)));
        }

        return result.OrderBy(x => x.Supplier).ThenBy(x => x.ArticleNumber).ToArray();
    }
}
