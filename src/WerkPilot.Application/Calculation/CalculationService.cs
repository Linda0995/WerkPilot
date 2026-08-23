using WerkPilot.Application.Auditing;
using WerkPilot.Domain.Calculation;
using WerkPilot.Application.Materials;

namespace WerkPilot.Application.Calculation;

public sealed class CalculationService(
    ICalculationRepository repository,
    IMaterialRepository materialRepository,
    IAuditTrail auditTrail)
{
    public async Task<OfferCalculationDto> GetOrCreateAsync(
        Guid offerId,
        CancellationToken cancellationToken = default)
    {
        var calculation = await repository.GetByOfferIdAsync(offerId, cancellationToken);

        if (calculation is null)
        {
            calculation = new OfferCalculation(offerId);
            await repository.AddAsync(calculation, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);
        }

        return Map(calculation);
    }

    public async Task AddItemAsync(
        Guid offerId,
        CostType costType,
        string description,
        decimal quantity,
        decimal unitCost,
        Guid? materialItemId = null,
        CancellationToken cancellationToken = default)
    {
        var calculation = await GetRequiredAsync(offerId, cancellationToken);
        calculation.AddItem(costType, description, quantity, unitCost, materialItemId);
        await repository.SaveChangesAsync(cancellationToken);

        await auditTrail.WriteAsync(
            "OfferCalculation",
            calculation.Id,
            "ItemAdded",
            $"Kalkulationsposition „{description.Trim()}“ wurde hinzugefügt.",
            cancellationToken);
    }

    public async Task AddMaterialAsync(
        Guid offerId,
        Guid materialItemId,
        decimal quantity,
        CancellationToken cancellationToken = default)
    {
        var material = await materialRepository.GetAsync(materialItemId, cancellationToken)
            ?? throw new InvalidOperationException("Materialartikel wurde nicht gefunden.");

        if (!material.IsActive)
            throw new InvalidOperationException("Inaktive Materialartikel können nicht übernommen werden.");

        await AddItemAsync(
            offerId,
            CostType.Material,
            $"{material.ArticleNumber} – {material.Description}",
            quantity,
            material.PurchasePrice,
            material.Id,
            cancellationToken);
    }

    public async Task UpdateItemAsync(
        Guid offerId,
        Guid itemId,
        CostType costType,
        string description,
        decimal quantity,
        decimal unitCost,
        CancellationToken cancellationToken = default)
    {
        var calculation = await GetRequiredAsync(offerId, cancellationToken);
        calculation.UpdateItem(itemId, costType, description, quantity, unitCost);
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveItemAsync(
        Guid offerId,
        Guid itemId,
        CancellationToken cancellationToken = default)
    {
        var calculation = await GetRequiredAsync(offerId, cancellationToken);
        calculation.RemoveItem(itemId);
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task SetProfitTargetAsync(
        Guid offerId,
        decimal percent,
        CancellationToken cancellationToken = default)
    {
        var calculation = await GetRequiredAsync(offerId, cancellationToken);
        calculation.SetProfitTarget(percent);
        await repository.SaveChangesAsync(cancellationToken);

        await auditTrail.WriteAsync(
            "OfferCalculation",
            calculation.Id,
            "ProfitTargetChanged",
            $"Firmenziel wurde auf {percent:N2} % gesetzt.",
            cancellationToken);
    }

    private async Task<OfferCalculation> GetRequiredAsync(
        Guid offerId,
        CancellationToken cancellationToken)
    {
        var calculation = await repository.GetByOfferIdAsync(offerId, cancellationToken);
        if (calculation is not null)
            return calculation;

        calculation = new OfferCalculation(offerId);
        await repository.AddAsync(calculation, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return calculation;
    }

    private static OfferCalculationDto Map(OfferCalculation calculation) => new(
        calculation.Id,
        calculation.OfferId,
        calculation.ProfitTargetPercent,
        calculation.MaterialCost,
        calculation.LaborCost,
        calculation.ExternalServiceCost,
        calculation.OverheadCost,
        calculation.TotalCost,
        calculation.TargetProfitAmount,
        calculation.RecommendedNetPrice,
        calculation.Items
            .OrderBy(x => x.PositionNumber)
            .Select(x => new CalculationItemDto(
                x.Id,
                x.PositionNumber,
                x.CostType,
                x.Description,
                x.Quantity,
                x.UnitCost,
                x.TotalCost,
                x.MaterialItemId))
            .ToArray());
}
