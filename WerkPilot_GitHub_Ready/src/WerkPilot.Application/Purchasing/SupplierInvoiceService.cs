using WerkPilot.Application.Auditing;
using WerkPilot.Application.Identity;
using WerkPilot.Domain.Purchasing;

namespace WerkPilot.Application.Purchasing;

public sealed class SupplierInvoiceService(
    ISupplierInvoiceRepository invoiceRepository,
    ISupplierOrderRepository orderRepository,
    SessionContext session,
    ISupplierInvoiceCsvExporter csvExporter,
    IAuditTrail auditTrail)
{
    private const decimal PriceTolerancePercent = 2m;

    public async Task<IReadOnlyList<SupplierInvoiceDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var invoices = await invoiceRepository.GetAllAsync(cancellationToken);
        var result = new List<SupplierInvoiceDto>();

        foreach (var invoice in invoices)
            result.Add(await MapAsync(invoice, cancellationToken));

        return result
            .OrderByDescending(x => x.InvoiceDate)
            .ThenByDescending(x => x.InvoiceNumber)
            .ToArray();
    }

    public async Task<SupplierInvoiceDto> CreateFromOrderAsync(
        Guid supplierOrderId,
        string invoiceNumber,
        DateOnly invoiceDate,
        DateOnly dueDate,
        decimal cashDiscountPercent = 0m,
        DateOnly? cashDiscountDueDate = null,
        CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetAsync(supplierOrderId, cancellationToken)
            ?? throw new InvalidOperationException("Lieferantenbestellung wurde nicht gefunden.");

        if (order.Status is SupplierOrderStatus.Draft or SupplierOrderStatus.Cancelled)
            throw new InvalidOperationException(
                "Nur bestellte oder gelieferte Bestellungen können verrechnet werden.");

        if (await invoiceRepository.InvoiceNumberExistsAsync(
                order.SupplierName,
                invoiceNumber,
                cancellationToken))
        {
            throw new InvalidOperationException(
                "Diese Rechnungsnummer wurde für den Lieferanten bereits erfasst.");
        }

        var invoice = new SupplierInvoice(
            invoiceNumber,
            order.Id,
            order.SupplierName,
            invoiceDate,
            dueDate,
            session.DisplayName,
            cashDiscountPercent,
            cashDiscountDueDate);

        foreach (var line in order.Lines)
        {
            var invoiceQuantity = line.ReceivedQuantity > 0
                ? line.ReceivedQuantity
                : line.OrderedQuantity;

            invoice.AddLine(
                line.Id,
                line.MaterialItemId,
                line.ArticleNumber,
                line.Description,
                invoiceQuantity,
                line.UnitPriceNet);
        }

        await invoiceRepository.AddAsync(invoice, cancellationToken);
        await invoiceRepository.SaveChangesAsync(cancellationToken);

        await auditTrail.WriteAsync(
            "SupplierInvoice",
            invoice.Id,
            "Created",
            $"Eingangsrechnung {invoice.InvoiceNumber} wurde aus Bestellung {order.OrderNumber} erzeugt.",
            cancellationToken);

        return await MapAsync(invoice, cancellationToken);
    }

    public async Task UpdateLineAsync(
        Guid invoiceId,
        Guid lineId,
        decimal invoicedQuantity,
        decimal unitPriceNet,
        CancellationToken cancellationToken = default)
    {
        var invoice = await GetRequiredAsync(invoiceId, cancellationToken);
        invoice.UpdateLine(lineId, invoicedQuantity, unitPriceNet);
        await invoiceRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task SubmitForReviewAsync(
        Guid invoiceId,
        CancellationToken cancellationToken = default)
    {
        var invoice = await GetRequiredAsync(invoiceId, cancellationToken);
        invoice.SubmitForReview();
        await invoiceRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task ApproveAsync(
        Guid invoiceId,
        string? reviewNote,
        bool allowWarnings,
        CancellationToken cancellationToken = default)
    {
        var invoice = await GetRequiredAsync(invoiceId, cancellationToken);
        var dto = await MapAsync(invoice, cancellationToken);

        if (dto.CriticalCount > 0)
            throw new InvalidOperationException(
                "Die Rechnung enthält kritische Abweichungen und kann nicht freigegeben werden.");

        if (dto.WarningCount > 0 && !allowWarnings)
            throw new InvalidOperationException(
                "Die Rechnung enthält Warnungen. Die Freigabe muss ausdrücklich bestätigt werden.");

        invoice.Approve(session.DisplayName, reviewNote);
        await invoiceRepository.SaveChangesAsync(cancellationToken);

        await auditTrail.WriteAsync(
            "SupplierInvoice",
            invoice.Id,
            "Approved",
            $"Eingangsrechnung {invoice.InvoiceNumber} wurde freigegeben.",
            cancellationToken);
    }

    public async Task RejectAsync(
        Guid invoiceId,
        string reviewNote,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reviewNote))
            throw new ArgumentException("Ablehnungsgrund erforderlich.", nameof(reviewNote));

        var invoice = await GetRequiredAsync(invoiceId, cancellationToken);
        invoice.Reject(reviewNote);
        await invoiceRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkPaidAsync(
        Guid invoiceId,
        CancellationToken cancellationToken = default)
    {
        var invoice = await GetRequiredAsync(invoiceId, cancellationToken);
        invoice.MarkPaid();
        await invoiceRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task CancelAsync(
        Guid invoiceId,
        CancellationToken cancellationToken = default)
    {
        var invoice = await GetRequiredAsync(invoiceId, cancellationToken);
        invoice.Cancel();
        await invoiceRepository.SaveChangesAsync(cancellationToken);
    }


    public async Task RegisterPaymentAsync(
        Guid invoiceId,
        decimal amount,
        DateOnly paymentDate,
        string? reference,
        CancellationToken cancellationToken = default)
    {
        var invoice = await GetRequiredAsync(invoiceId, cancellationToken);

        invoice.RegisterPayment(
            amount,
            paymentDate,
            reference,
            session.DisplayName);

        await invoiceRepository.SaveChangesAsync(cancellationToken);

        await auditTrail.WriteAsync(
            "SupplierInvoice",
            invoice.Id,
            "PaymentRegistered",
            $"Zahlung über {amount:N2} € wurde für Rechnung {invoice.InvoiceNumber} erfasst.",
            cancellationToken);
    }

    public async Task<SupplierInvoiceLiquiditySummaryDto> GetLiquiditySummaryAsync(
        DateOnly today,
        CancellationToken cancellationToken = default)
    {
        var invoices = await invoiceRepository.GetAllAsync(cancellationToken);

        var items = invoices
            .Where(x =>
                x.Status is SupplierInvoiceStatus.Approved or SupplierInvoiceStatus.Paid &&
                x.OpenAmount > 0m)
            .Select(x =>
            {
                var days = x.DueDate.DayNumber - today.DayNumber;
                var discountAvailable =
                    x.CashDiscountPercent > 0m &&
                    x.CashDiscountDueDate.HasValue &&
                    x.CashDiscountDueDate.Value >= today;

                return new SupplierInvoiceLiquidityItemDto(
                    x.Id,
                    x.InvoiceNumber,
                    x.SupplierName,
                    x.DueDate,
                    x.OpenAmount,
                    x.DueDate < today,
                    days,
                    discountAvailable,
                    x.CashDiscountDueDate,
                    discountAvailable ? x.CashDiscountAmount : 0m,
                    discountAvailable
                        ? Math.Max(0m, x.OpenAmount - x.CashDiscountAmount)
                        : x.OpenAmount);
            })
            .OrderBy(x => x.DueDate)
            .ThenBy(x => x.SupplierName)
            .ToArray();

        return new SupplierInvoiceLiquiditySummaryDto(
            items.Sum(x => x.OpenAmount),
            items.Where(x => x.IsOverdue).Sum(x => x.OpenAmount),
            items.Where(x => x.DaysUntilDue >= 0 && x.DaysUntilDue <= 7).Sum(x => x.OpenAmount),
            items.Where(x => x.DaysUntilDue >= 0 && x.DaysUntilDue <= 14).Sum(x => x.OpenAmount),
            items.Where(x => x.DaysUntilDue >= 0 && x.DaysUntilDue <= 30).Sum(x => x.OpenAmount),
            items.Where(x => x.CashDiscountAvailable).Sum(x => x.CashDiscountAmount),
            items.Length,
            items.Count(x => x.IsOverdue),
            items);
    }

    public async Task<string> ExportCsvAsync(
        Guid invoiceId,
        CancellationToken cancellationToken = default) =>
        csvExporter.Export(
            await MapAsync(
                await GetRequiredAsync(invoiceId, cancellationToken),
                cancellationToken));

    private async Task<SupplierInvoice> GetRequiredAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        await invoiceRepository.GetAsync(id, cancellationToken)
        ?? throw new InvalidOperationException("Eingangsrechnung wurde nicht gefunden.");

    private async Task<SupplierInvoiceDto> MapAsync(
        SupplierInvoice invoice,
        CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetAsync(
            invoice.SupplierOrderId,
            cancellationToken)
            ?? throw new InvalidOperationException("Zugehörige Bestellung wurde nicht gefunden.");

        var lines = new List<SupplierInvoiceLineDto>();

        foreach (var invoiceLine in invoice.Lines)
        {
            var orderLine = order.Lines.SingleOrDefault(
                x => x.Id == invoiceLine.SupplierOrderLineId)
                ?? throw new InvalidOperationException("Bestellposition wurde nicht gefunden.");

            var quantityVariance = decimal.Round(
                invoiceLine.InvoicedQuantity - orderLine.ReceivedQuantity,
                3,
                MidpointRounding.AwayFromZero);

            var priceVariance = decimal.Round(
                invoiceLine.UnitPriceNet - orderLine.UnitPriceNet,
                4,
                MidpointRounding.AwayFromZero);

            var valueVariance = decimal.Round(
                invoiceLine.LineTotalNet -
                (invoiceLine.InvoicedQuantity * orderLine.UnitPriceNet),
                2,
                MidpointRounding.AwayFromZero);

            var priceVariancePercent = orderLine.UnitPriceNet == 0
                ? (invoiceLine.UnitPriceNet == 0 ? 0m : 100m)
                : Math.Abs(priceVariance / orderLine.UnitPriceNet * 100m);

            var matchStatus =
                invoiceLine.InvoicedQuantity > orderLine.ReceivedQuantity
                    ? SupplierInvoiceMatchStatus.Critical
                    : quantityVariance != 0 || priceVariancePercent > PriceTolerancePercent
                        ? SupplierInvoiceMatchStatus.Warning
                        : SupplierInvoiceMatchStatus.Exact;

            lines.Add(new SupplierInvoiceLineDto(
                invoiceLine.Id,
                invoiceLine.SupplierOrderLineId,
                invoiceLine.MaterialItemId,
                invoiceLine.ArticleNumber,
                invoiceLine.Description,
                orderLine.OrderedQuantity,
                orderLine.ReceivedQuantity,
                invoiceLine.InvoicedQuantity,
                orderLine.OpenQuantity,
                orderLine.UnitPriceNet,
                invoiceLine.UnitPriceNet,
                quantityVariance,
                priceVariance,
                valueVariance,
                invoiceLine.LineTotalNet,
                matchStatus));
        }

        var warningCount = lines.Count(x => x.MatchStatus == SupplierInvoiceMatchStatus.Warning);
        var criticalCount = lines.Count(x => x.MatchStatus == SupplierInvoiceMatchStatus.Critical);
        var status = criticalCount > 0
            ? SupplierInvoiceMatchStatus.Critical
            : warningCount > 0
                ? SupplierInvoiceMatchStatus.Warning
                : SupplierInvoiceMatchStatus.Exact;

        return new SupplierInvoiceDto(
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.SupplierOrderId,
            order.OrderNumber,
            invoice.SupplierName,
            invoice.InvoiceDate,
            invoice.DueDate,
            invoice.CreatedBy,
            invoice.Status,
            invoice.ReviewNote,
            invoice.ApprovedBy,
            invoice.ApprovedAtUtc,
            invoice.PaidAtUtc,
            invoice.CashDiscountPercent,
            invoice.CashDiscountDueDate,
            invoice.CashDiscountAmount,
            invoice.DiscountedPayableAmount,
            invoice.TotalNet,
            invoice.PaidAmount,
            invoice.OpenAmount,
            decimal.Round(lines.Sum(x => x.ValueVariance), 2, MidpointRounding.AwayFromZero),
            warningCount,
            criticalCount,
            status,
            lines,
            invoice.Payments
                .OrderByDescending(x => x.PaymentDate)
                .Select(x => new SupplierInvoicePaymentDto(
                    x.Id,
                    x.Amount,
                    x.PaymentDate,
                    x.Reference,
                    x.CreatedBy,
                    x.CreatedAtUtc))
                .ToArray());
    }
}
