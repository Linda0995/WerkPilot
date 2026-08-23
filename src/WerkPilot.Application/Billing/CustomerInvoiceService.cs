using WerkPilot.Application.Auditing;
using WerkPilot.Application.Customers;
using WerkPilot.Application.Identity;
using WerkPilot.Application.Offers;
using WerkPilot.Application.Projects;
using WerkPilot.Domain.Billing;

namespace WerkPilot.Application.Billing;

public sealed class CustomerInvoiceService(
    ICustomerInvoiceRepository repository,
    CustomerService customerService,
    OfferService offerService,
    ProjectService projectService,
    SessionContext session,
    ICustomerInvoiceCsvExporter csvExporter,
    ICustomerInvoicePdfExporter pdfExporter,
    DocumentArchiveService documentArchive,
    WerkPilot.Application.Settings.CompanyProfileService companyProfileService,
    IAuditTrail auditTrail)
{
    public async Task<IReadOnlyList<CustomerInvoiceDto>> GetAllAsync(
        DateOnly today,
        CancellationToken cancellationToken = default) =>
        (await repository.GetAllAsync(cancellationToken))
            .OrderByDescending(x => x.InvoiceDate)
            .ThenByDescending(x => x.InvoiceNumber)
            .Select(x => Map(x, today))
            .ToArray();

    public async Task<CustomerInvoiceDto> CreateFromOfferAsync(
        Guid offerId,
        DateOnly invoiceDate,
        DateOnly dueDate,
        decimal vatRatePercent,
        CancellationToken cancellationToken = default)
    {
        var offer = await offerService.GetAsync(offerId, cancellationToken);
        var customers = await customerService.SearchAsync(
            null,
            includeDeleted: true,
            cancellationToken);

        var customer = customers.SingleOrDefault(x => x.Id == offer.CustomerId)
            ?? throw new InvalidOperationException("Kunde wurde nicht gefunden.");

        var invoice = new CustomerInvoice(
            await repository.GetNextNumberAsync(invoiceDate.Year, cancellationToken),
            customer.Id,
            customer.DisplayName,
            invoiceDate,
            dueDate,
            offer.Id,
            null,
            session.DisplayName);

        foreach (var item in offer.Positions)
        {
            invoice.AddLine(
                item.Description,
                item.Quantity,
                "Stk.",
                item.UnitPriceNet,
                vatRatePercent);
        }

        await repository.AddAsync(invoice, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        await auditTrail.WriteAsync(
            "CustomerInvoice",
            invoice.Id,
            "Created",
            $"Ausgangsrechnung {invoice.InvoiceNumber} wurde aus Angebot {offer.OfferNumber} erzeugt.",
            cancellationToken);

        return Map(invoice, DateOnly.FromDateTime(DateTime.Today));
    }

    public async Task<CustomerInvoiceDto> CreateFromProjectAsync(
        Guid projectId,
        DateOnly invoiceDate,
        DateOnly dueDate,
        decimal vatRatePercent,
        CancellationToken cancellationToken = default)
    {
        var projects = await projectService.GetAllAsync(cancellationToken);
        var project = projects.SingleOrDefault(x => x.Id == projectId)
            ?? throw new InvalidOperationException("Projekt wurde nicht gefunden.");

        if (!project.SourceOfferId.HasValue)
            throw new InvalidOperationException(
                "Für das Projekt ist kein Ursprungsangebot vorhanden.");

        var result = await CreateFromOfferAsync(
            project.SourceOfferId.Value,
            invoiceDate,
            dueDate,
            vatRatePercent,
            cancellationToken);

        var invoice = await GetRequiredAsync(result.Id, cancellationToken);

        typeof(CustomerInvoice)
            .GetProperty(nameof(CustomerInvoice.SourceProjectId))!
            .SetValue(invoice, project.Id);

        await repository.SaveChangesAsync(cancellationToken);
        return Map(invoice, DateOnly.FromDateTime(DateTime.Today));
    }

    public async Task IssueAsync(
        Guid invoiceId,
        CancellationToken cancellationToken = default)
    {
        var invoice = await GetRequiredAsync(invoiceId, cancellationToken);
        invoice.Issue();
        await repository.SaveChangesAsync(cancellationToken);
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

        await repository.SaveChangesAsync(cancellationToken);

        await auditTrail.WriteAsync(
            "CustomerInvoice",
            invoice.Id,
            "PaymentRegistered",
            $"Zahlung über {amount:N2} € wurde erfasst.",
            cancellationToken);
    }

    public async Task AdvanceDunningAsync(
        Guid invoiceId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var invoice = await GetRequiredAsync(invoiceId, cancellationToken);
        invoice.AdvanceDunning(date);
        await repository.SaveChangesAsync(cancellationToken);

        await auditTrail.WriteAsync(
            "CustomerInvoice",
            invoice.Id,
            "DunningAdvanced",
            $"Mahnstufe wurde auf {invoice.DunningLevel} gesetzt.",
            cancellationToken);
    }

    public async Task CancelAsync(
        Guid invoiceId,
        CancellationToken cancellationToken = default)
    {
        var invoice = await GetRequiredAsync(invoiceId, cancellationToken);
        invoice.Cancel();
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<ReceivablesSummaryDto> GetReceivablesSummaryAsync(
        DateOnly today,
        CancellationToken cancellationToken = default)
    {
        var items = (await repository.GetAllAsync(cancellationToken))
            .Where(x =>
                x.Status is CustomerInvoiceStatus.Issued or CustomerInvoiceStatus.PartiallyPaid &&
                x.OpenAmount > 0m)
            .Select(x => Map(x, today))
            .OrderBy(x => x.DueDate)
            .ThenBy(x => x.CustomerName)
            .ToArray();

        return new ReceivablesSummaryDto(
            items.Sum(x => x.OpenAmount),
            items.Where(x => x.IsOverdue).Sum(x => x.OpenAmount),
            items.Where(x => !x.IsOverdue && x.DueDate.DayNumber - today.DayNumber <= 7)
                .Sum(x => x.OpenAmount),
            items.Where(x => !x.IsOverdue && x.DueDate.DayNumber - today.DayNumber <= 14)
                .Sum(x => x.OpenAmount),
            items.Where(x => !x.IsOverdue && x.DueDate.DayNumber - today.DayNumber <= 30)
                .Sum(x => x.OpenAmount),
            items.Length,
            items.Count(x => x.IsOverdue),
            items);
    }

    public async Task<DocumentArchiveResult> ExportPdfAsync(
        Guid invoiceId,
        string destinationDirectory,
        CancellationToken cancellationToken = default)
    {
        var invoice = Map(
            await GetRequiredAsync(invoiceId, cancellationToken),
            DateOnly.FromDateTime(DateTime.Today));

        var company = await companyProfileService.GetAsync(cancellationToken);
        var path = await pdfExporter.ExportAsync(
            new CustomerInvoiceDocumentData(invoice, company),
            destinationDirectory,
            cancellationToken);

        return await documentArchive.ArchiveAsync(
            path,
            "CustomerInvoice",
            invoice.InvoiceNumber,
            cancellationToken);
    }

    public async Task<string> ExportCsvAsync(
        Guid invoiceId,
        CancellationToken cancellationToken = default) =>
        csvExporter.Export(
            Map(
                await GetRequiredAsync(invoiceId, cancellationToken),
                DateOnly.FromDateTime(DateTime.Today)));

    private async Task<CustomerInvoice> GetRequiredAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        await repository.GetAsync(id, cancellationToken)
        ?? throw new InvalidOperationException("Ausgangsrechnung wurde nicht gefunden.");

    private static CustomerInvoiceDto Map(
        CustomerInvoice x,
        DateOnly today)
    {
        var overdue = x.IsOverdue(today);

        return new CustomerInvoiceDto(
            x.Id,
            x.InvoiceNumber,
            x.CustomerId,
            x.CustomerName,
            x.InvoiceDate,
            x.DueDate,
            x.SourceOfferId,
            x.SourceProjectId,
            x.CreatedBy,
            x.Status,
            x.DunningLevel,
            x.LastDunningDate,
            x.IssuedAtUtc,
            x.PaidAtUtc,
            x.NetTotal,
            x.VatTotal,
            x.GrossTotal,
            x.PaidAmount,
            x.CreditedAmount,
            x.OpenAmount,
            overdue,
            overdue ? today.DayNumber - x.DueDate.DayNumber : 0,
            x.Lines
                .Select(line => new CustomerInvoiceLineDto(
                    line.Id,
                    line.Description,
                    line.Quantity,
                    line.Unit,
                    line.UnitPriceNet,
                    line.VatRatePercent,
                    line.NetTotal,
                    line.VatAmount,
                    line.GrossTotal))
                .ToArray(),
            x.Payments
                .OrderByDescending(payment => payment.PaymentDate)
                .Select(payment => new CustomerInvoicePaymentDto(
                    payment.Id,
                    payment.Amount,
                    payment.PaymentDate,
                    payment.Reference,
                    payment.CreatedBy,
                    payment.CreatedAtUtc))
                .ToArray());
    }
}
