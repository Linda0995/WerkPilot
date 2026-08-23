using WerkPilot.Application.Auditing;
using WerkPilot.Domain.Offers;

namespace WerkPilot.Application.Offers;

public sealed class OfferService(IOfferRepository repository, IAuditTrail auditTrail)
{
    public async Task<OfferDetailsDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var offer = await GetRequiredAsync(id, cancellationToken);
        return MapDetails(offer);
    }

    public async Task<IReadOnlyList<OfferDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        (await repository.GetAllAsync(cancellationToken)).Select(Map).ToArray();

    public async Task<OfferDto> CreateAsync(
        Guid customerId,
        string title,
        DateOnly validUntil,
        decimal taxRate = 20m,
        CancellationToken cancellationToken = default)
    {
        var number = await repository.GetNextOfferNumberAsync(DateTime.Today.Year, cancellationToken);
        var offer = new Offer(number, customerId, title, validUntil, taxRate);
        await repository.AddAsync(offer, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        await auditTrail.WriteAsync("Offer", offer.Id, "Created", $"Angebot {number} wurde angelegt.", cancellationToken);
        return Map(offer);
    }

    public async Task AddPositionAsync(
        Guid offerId,
        string description,
        decimal quantity,
        decimal unitPriceNet,
        bool isOptional = false,
        CancellationToken cancellationToken = default)
    {
        var offer = await GetRequiredAsync(offerId, cancellationToken);
        offer.AddPosition(description, quantity, unitPriceNet, isOptional);
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdatePositionAsync(
        UpdateOfferPositionRequest request,
        CancellationToken cancellationToken = default)
    {
        var offer = await GetRequiredAsync(request.OfferId, cancellationToken);
        offer.UpdatePosition(
            request.PositionId,
            request.Description,
            request.Quantity,
            request.UnitPriceNet,
            request.IsOptional);

        await repository.SaveChangesAsync(cancellationToken);
        await auditTrail.WriteAsync(
            "Offer",
            offer.Id,
            "PositionUpdated",
            $"Position {request.PositionId} wurde geändert.",
            cancellationToken);
    }

    public async Task SetDiscountAsync(
        Guid offerId,
        decimal discountPercent,
        CancellationToken cancellationToken = default)
    {
        var offer = await GetRequiredAsync(offerId, cancellationToken);
        offer.SetDiscount(discountPercent);
        await repository.SaveChangesAsync(cancellationToken);
        await auditTrail.WriteAsync(
            "Offer",
            offer.Id,
            "DiscountChanged",
            $"Angebotsrabatt wurde auf {discountPercent:N2} % gesetzt.",
            cancellationToken);
    }

    public async Task RemovePositionAsync(
        Guid offerId,
        Guid positionId,
        CancellationToken cancellationToken = default)
    {
        var offer = await GetRequiredAsync(offerId, cancellationToken);
        offer.RemovePosition(positionId);
        await repository.SaveChangesAsync(cancellationToken);
        await auditTrail.WriteAsync(
            "Offer",
            offer.Id,
            "PositionRemoved",
            $"Position {positionId} wurde entfernt.",
            cancellationToken);
    }

    public async Task<OfferDto> DuplicateAsync(
        Guid sourceOfferId,
        DateOnly validUntil,
        CancellationToken cancellationToken = default)
    {
        var source = await GetRequiredAsync(sourceOfferId, cancellationToken);
        var number = await repository.GetNextOfferNumberAsync(DateTime.Today.Year, cancellationToken);
        var copy = source.CreateCopy(number, validUntil);

        await repository.AddAsync(copy, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        await auditTrail.WriteAsync(
            "Offer",
            copy.Id,
            "Duplicated",
            $"Angebot {source.OfferNumber} wurde als {copy.OfferNumber} kopiert.",
            cancellationToken);

        return Map(copy);
    }

    public async Task<int> MarkExpiredAsync(
        DateOnly today,
        CancellationToken cancellationToken = default)
    {
        var offers = await repository.GetAllAsync(cancellationToken);
        var changed = 0;

        foreach (var offer in offers)
        {
            var previous = offer.Status;
            offer.MarkExpired(today);
            if (previous != offer.Status)
                changed++;
        }

        if (changed > 0)
            await repository.SaveChangesAsync(cancellationToken);

        return changed;
    }

    public async Task MarkSentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var offer = await GetRequiredAsync(id, cancellationToken);
        offer.MarkSent();
        await repository.SaveChangesAsync(cancellationToken);
        await auditTrail.WriteAsync("Offer", offer.Id, "Sent", "Angebot wurde als gesendet markiert.", cancellationToken);
    }

    public async Task AcceptAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var offer = await GetRequiredAsync(id, cancellationToken);
        offer.Accept();
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task RejectAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var offer = await GetRequiredAsync(id, cancellationToken);
        offer.Reject();
        await repository.SaveChangesAsync(cancellationToken);
    }

    private async Task<Offer> GetRequiredAsync(Guid id, CancellationToken cancellationToken) =>
        await repository.GetAsync(id, cancellationToken)
        ?? throw new InvalidOperationException("Angebot wurde nicht gefunden.");

    private static OfferDto Map(Offer x) => new(
        x.Id, x.OfferNumber, x.CustomerId, x.Title, x.OfferDate, x.ValidUntil,
        x.Status, x.NetTotal, x.TaxTotal, x.GrossTotal, x.Positions.Count);

    private static OfferDetailsDto MapDetails(Offer x) => new(
        x.Id,
        x.OfferNumber,
        x.CustomerId,
        x.Title,
        x.OfferDate,
        x.ValidUntil,
        x.Status,
        x.TaxRate,
        x.DiscountPercent,
        x.PositionsNetTotal,
        x.DiscountAmount,
        x.NetTotal,
        x.TaxTotal,
        x.GrossTotal,
        x.Positions
            .OrderBy(p => p.PositionNumber)
            .Select(p => new OfferPositionDto(
                p.Id,
                p.PositionNumber,
                p.Description,
                p.Quantity,
                p.UnitPriceNet,
                p.TotalNet,
                p.IsOptional))
            .ToArray());
}
