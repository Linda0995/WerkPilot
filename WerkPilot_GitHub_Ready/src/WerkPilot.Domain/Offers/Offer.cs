using WerkPilot.Domain.Common;

namespace WerkPilot.Domain.Offers;

public sealed class Offer : Entity
{
    private readonly List<OfferPosition> _positions = [];
    private Offer() { }

    public Offer(string offerNumber, Guid customerId, string title, DateOnly validUntil, decimal taxRate)
    {
        if (string.IsNullOrWhiteSpace(offerNumber)) throw new ArgumentException("Angebotsnummer erforderlich.", nameof(offerNumber));
        if (customerId == Guid.Empty) throw new ArgumentException("Kunde erforderlich.", nameof(customerId));
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Titel erforderlich.", nameof(title));
        if (taxRate < 0 || taxRate > 100) throw new ArgumentOutOfRangeException(nameof(taxRate));

        OfferNumber = offerNumber.Trim();
        CustomerId = customerId;
        Title = title.Trim();
        OfferDate = DateOnly.FromDateTime(DateTime.Today);
        ValidUntil = validUntil;
        TaxRate = taxRate;
        Status = OfferStatus.Draft;
    }

    public string OfferNumber { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public DateOnly OfferDate { get; private set; }
    public DateOnly ValidUntil { get; private set; }
    public decimal TaxRate { get; private set; }
    public OfferStatus Status { get; private set; }
    public decimal DiscountPercent { get; private set; }
    public IReadOnlyCollection<OfferPosition> Positions => _positions.AsReadOnly();
    public decimal PositionsNetTotal => _positions.Where(x => !x.IsOptional).Sum(x => x.TotalNet);
    public decimal DiscountAmount => decimal.Round(
        PositionsNetTotal * DiscountPercent / 100m,
        2,
        MidpointRounding.AwayFromZero);
    public decimal NetTotal => PositionsNetTotal - DiscountAmount;
    public decimal TaxTotal => decimal.Round(NetTotal * TaxRate / 100m, 2, MidpointRounding.AwayFromZero);
    public decimal GrossTotal => NetTotal + TaxTotal;

    public OfferPosition AddPosition(string description, decimal quantity, decimal unitPriceNet, bool isOptional = false)
    {
        EnsureEditable();
        var position = new OfferPosition(_positions.Count + 1, description, quantity, unitPriceNet, isOptional);
        _positions.Add(position);
        UpdatedAtUtc = DateTimeOffset.UtcNow;
        return position;
    }

    public void UpdatePosition(Guid positionId, string description, decimal quantity, decimal unitPriceNet, bool isOptional)
    {
        EnsureEditable();
        var position = _positions.SingleOrDefault(x => x.Id == positionId)
            ?? throw new InvalidOperationException("Angebotsposition wurde nicht gefunden.");

        position.Update(description, quantity, unitPriceNet, isOptional);
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void RemovePosition(Guid positionId)
    {
        EnsureEditable();
        var position = _positions.SingleOrDefault(x => x.Id == positionId)
            ?? throw new InvalidOperationException("Angebotsposition wurde nicht gefunden.");

        _positions.Remove(position);

        for (var index = 0; index < _positions.Count; index++)
            _positions[index].SetPositionNumber(index + 1);

        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Offer CreateCopy(string newOfferNumber, DateOnly validUntil)
    {
        var copy = new Offer(newOfferNumber, CustomerId, $"{Title} – Kopie", validUntil, TaxRate);

        foreach (var position in _positions.OrderBy(x => x.PositionNumber))
            copy.AddPosition(position.Description, position.Quantity, position.UnitPriceNet, position.IsOptional);

        copy.SetDiscount(DiscountPercent);
        return copy;
    }

    public void SetDiscount(decimal discountPercent)
    {
        EnsureEditable();

        if (discountPercent < 0 || discountPercent > 100)
            throw new ArgumentOutOfRangeException(nameof(discountPercent));

        DiscountPercent = discountPercent;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void MarkExpired(DateOnly today)
    {
        if (Status == OfferStatus.Sent && ValidUntil < today)
        {
            Status = OfferStatus.Expired;
            UpdatedAtUtc = DateTimeOffset.UtcNow;
        }
    }

    public void MarkSent()
    {
        if (_positions.Count == 0) throw new InvalidOperationException("Ein Angebot ohne Positionen kann nicht gesendet werden.");
        if (Status != OfferStatus.Draft) throw new InvalidOperationException("Nur Entwürfe können gesendet werden.");
        Status = OfferStatus.Sent;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Accept()
    {
        if (Status != OfferStatus.Sent) throw new InvalidOperationException("Nur gesendete Angebote können angenommen werden.");
        Status = OfferStatus.Accepted;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Reject()
    {
        if (Status != OfferStatus.Sent) throw new InvalidOperationException("Nur gesendete Angebote können abgelehnt werden.");
        Status = OfferStatus.Rejected;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private void EnsureEditable()
    {
        if (Status != OfferStatus.Draft)
            throw new InvalidOperationException("Nur Angebotsentwürfe können bearbeitet werden.");
    }
}
