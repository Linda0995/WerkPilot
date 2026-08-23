using WerkPilot.Application.Auditing;
using WerkPilot.Application.Identity;
using WerkPilot.Domain.Billing;

namespace WerkPilot.Application.Billing;

public sealed class CustomerCreditNoteService(
    ICustomerCreditNoteRepository creditNoteRepository,
    ICustomerInvoiceRepository invoiceRepository,
    SessionContext session,
    ICustomerCreditNoteCsvExporter csvExporter,
    ICustomerCreditNotePdfExporter pdfExporter,
    DocumentArchiveService documentArchive,
    WerkPilot.Application.Settings.CompanyProfileService companyProfileService,
    IAuditTrail auditTrail)
{
    public async Task<IReadOnlyList<CustomerCreditNoteDto>> GetAllAsync(
        CancellationToken cancellationToken = default) =>
        (await creditNoteRepository.GetAllAsync(cancellationToken))
            .OrderByDescending(x => x.CreditNoteDate)
            .ThenByDescending(x => x.CreditNoteNumber)
            .Select(Map)
            .ToArray();

    public async Task<CustomerCreditNoteDto> CreatePartialAsync(
        Guid customerInvoiceId,
        Guid sourceInvoiceLineId,
        decimal quantity,
        DateOnly creditNoteDate,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var invoice = await invoiceRepository.GetAsync(customerInvoiceId, cancellationToken)
            ?? throw new InvalidOperationException("Ausgangsrechnung wurde nicht gefunden.");

        ValidateInvoice(invoice);

        var sourceLine = invoice.Lines.SingleOrDefault(x => x.Id == sourceInvoiceLineId)
            ?? throw new InvalidOperationException("Rechnungsposition wurde nicht gefunden.");

        if (quantity <= 0m || quantity > sourceLine.Quantity)
            throw new InvalidOperationException(
                "Die Gutschriftsmenge muss größer als null sein und darf die Rechnungsmenge nicht überschreiten.");

        var creditNote = await CreateBaseAsync(invoice, creditNoteDate, reason, cancellationToken);
        creditNote.AddLine(
            sourceLine.Id,
            sourceLine.Description,
            quantity,
            sourceLine.Unit,
            sourceLine.UnitPriceNet,
            sourceLine.VatRatePercent);

        if (creditNote.GrossTotal > invoice.OpenAmount)
            throw new InvalidOperationException(
                "Der Gutschriftsbetrag überschreitet den offenen Rechnungsbetrag.");

        await creditNoteRepository.AddAsync(creditNote, cancellationToken);
        await creditNoteRepository.SaveChangesAsync(cancellationToken);
        await WriteCreatedAuditAsync(creditNote, cancellationToken);
        return Map(creditNote);
    }

    public async Task<CustomerCreditNoteDto> CreateFullAsync(
        Guid customerInvoiceId,
        DateOnly creditNoteDate,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var invoice = await invoiceRepository.GetAsync(customerInvoiceId, cancellationToken)
            ?? throw new InvalidOperationException("Ausgangsrechnung wurde nicht gefunden.");

        ValidateInvoice(invoice);

        var creditNote = await CreateBaseAsync(invoice, creditNoteDate, reason, cancellationToken);
        var remaining = invoice.OpenAmount;

        foreach (var sourceLine in invoice.Lines)
        {
            if (remaining <= 0m)
                break;

            var fullGross = sourceLine.GrossTotal;
            var factor = fullGross <= remaining ? 1m : remaining / fullGross;
            var quantity = decimal.Round(
                sourceLine.Quantity * factor,
                3,
                MidpointRounding.AwayFromZero);

            if (quantity <= 0m)
                continue;

            creditNote.AddLine(
                sourceLine.Id,
                sourceLine.Description,
                quantity,
                sourceLine.Unit,
                sourceLine.UnitPriceNet,
                sourceLine.VatRatePercent);

            remaining -= decimal.Round(
                quantity * sourceLine.UnitPriceNet *
                (1m + sourceLine.VatRatePercent / 100m),
                2,
                MidpointRounding.AwayFromZero);
        }

        if (creditNote.Lines.Count == 0)
            throw new InvalidOperationException("Es konnte keine Gutschriftsposition erzeugt werden.");

        await creditNoteRepository.AddAsync(creditNote, cancellationToken);
        await creditNoteRepository.SaveChangesAsync(cancellationToken);
        await WriteCreatedAuditAsync(creditNote, cancellationToken);
        return Map(creditNote);
    }

    public async Task IssueAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var note = await GetRequiredAsync(id, cancellationToken);
        note.Issue();
        await creditNoteRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task ApplyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var note = await GetRequiredAsync(id, cancellationToken);
        var invoice = await invoiceRepository.GetAsync(note.CustomerInvoiceId, cancellationToken)
            ?? throw new InvalidOperationException("Ausgangsrechnung wurde nicht gefunden.");

        invoice.ApplyCredit(note.GrossTotal);
        note.MarkApplied();

        await invoiceRepository.SaveChangesAsync(cancellationToken);
        await creditNoteRepository.SaveChangesAsync(cancellationToken);

        await auditTrail.WriteAsync(
            "CustomerCreditNote",
            note.Id,
            "Applied",
            $"Gutschrift {note.CreditNoteNumber} wurde mit Rechnung {invoice.InvoiceNumber} verrechnet.",
            cancellationToken);
    }

    public async Task CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var note = await GetRequiredAsync(id, cancellationToken);
        note.Cancel();
        await creditNoteRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<DocumentArchiveResult> ExportPdfAsync(
        Guid id,
        string destinationDirectory,
        CancellationToken cancellationToken = default)
    {
        var note = Map(await GetRequiredAsync(id, cancellationToken));
        var company = await companyProfileService.GetAsync(cancellationToken);
        var path = await pdfExporter.ExportAsync(
            new CustomerCreditNoteDocumentData(note, company),
            destinationDirectory,
            cancellationToken);

        return await documentArchive.ArchiveAsync(
            path,
            "CustomerCreditNote",
            note.CreditNoteNumber,
            cancellationToken);
    }

    public async Task<string> ExportCsvAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        csvExporter.Export(Map(await GetRequiredAsync(id, cancellationToken)));

    private async Task<CustomerCreditNote> CreateBaseAsync(
        CustomerInvoice invoice,
        DateOnly date,
        string reason,
        CancellationToken cancellationToken) =>
        new(
            await creditNoteRepository.GetNextNumberAsync(date.Year, cancellationToken),
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.CustomerId,
            invoice.CustomerName,
            date,
            reason,
            session.DisplayName);

    private static void ValidateInvoice(CustomerInvoice invoice)
    {
        if (invoice.Status is CustomerInvoiceStatus.Draft or CustomerInvoiceStatus.Cancelled)
            throw new InvalidOperationException(
                "Für Entwürfe oder stornierte Rechnungen kann keine Gutschrift erstellt werden.");
        if (invoice.OpenAmount <= 0m)
            throw new InvalidOperationException("Die Rechnung besitzt keinen offenen Betrag mehr.");
    }

    private Task WriteCreatedAuditAsync(
        CustomerCreditNote note,
        CancellationToken cancellationToken) =>
        auditTrail.WriteAsync(
            "CustomerCreditNote",
            note.Id,
            "Created",
            $"Gutschrift {note.CreditNoteNumber} wurde erstellt.",
            cancellationToken);

    private async Task<CustomerCreditNote> GetRequiredAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        await creditNoteRepository.GetAsync(id, cancellationToken)
        ?? throw new InvalidOperationException("Gutschrift wurde nicht gefunden.");

    private static CustomerCreditNoteDto Map(CustomerCreditNote x) => new(
        x.Id,
        x.CreditNoteNumber,
        x.CustomerInvoiceId,
        x.CustomerInvoiceNumber,
        x.CustomerId,
        x.CustomerName,
        x.CreditNoteDate,
        x.Reason,
        x.CreatedBy,
        x.Status,
        x.IssuedAtUtc,
        x.AppliedAtUtc,
        x.NetTotal,
        x.VatTotal,
        x.GrossTotal,
        x.Lines.Select(line => new CustomerCreditNoteLineDto(
            line.Id,
            line.SourceInvoiceLineId,
            line.Description,
            line.Quantity,
            line.Unit,
            line.UnitPriceNet,
            line.VatRatePercent,
            line.NetTotal,
            line.VatAmount,
            line.GrossTotal)).ToArray());
}
