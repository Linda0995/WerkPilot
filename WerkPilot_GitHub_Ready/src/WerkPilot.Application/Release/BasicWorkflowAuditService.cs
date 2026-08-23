using WerkPilot.Application.Billing;
using WerkPilot.Application.Calculation;
using WerkPilot.Application.Customers;
using WerkPilot.Application.Offers;
using WerkPilot.Application.Projects;
using WerkPilot.Domain.Billing;
using WerkPilot.Domain.Offers;

namespace WerkPilot.Application.Release;

public sealed class BasicWorkflowAuditService(
    CustomerService customerService,
    OfferService offerService,
    ProjectService projectService,
    CustomerInvoiceService invoiceService,
    DunningNoticeService dunningNoticeService,
    ICalculationRepository calculationRepository)
{
    public async Task<BasicWorkflowAuditDto> EvaluateAsync(
        DateOnly today,
        CancellationToken cancellationToken = default)
    {
        var customers = await customerService.SearchAsync(
            null,
            includeDeleted: false,
            cancellationToken);

        var offers = await offerService.GetAllAsync(cancellationToken);
        var projects = await projectService.GetAllAsync(cancellationToken);
        var invoices = await invoiceService.GetAllAsync(today, cancellationToken);
        var dunnings = await dunningNoticeService.GetAllAsync(cancellationToken);

        var customerById = customers.ToDictionary(x => x.Id);
        var projectByOfferId = projects
            .Where(x => x.SourceOfferId.HasValue)
            .GroupBy(x => x.SourceOfferId!.Value)
            .ToDictionary(
                x => x.Key,
                x => x.OrderByDescending(p => p.PlannedStart).First());

        var invoicesByOfferId = invoices
            .Where(x => x.SourceOfferId.HasValue)
            .GroupBy(x => x.SourceOfferId!.Value)
            .ToDictionary(
                x => x.Key,
                x => x.OrderByDescending(i => i.InvoiceDate).ToArray());

        var invoicesByProjectId = invoices
            .Where(x => x.SourceProjectId.HasValue)
            .GroupBy(x => x.SourceProjectId!.Value)
            .ToDictionary(
                x => x.Key,
                x => x.OrderByDescending(i => i.InvoiceDate).ToArray());

        var dunningByInvoiceId = dunnings
            .GroupBy(x => x.CustomerInvoiceId)
            .ToDictionary(x => x.Key, x => x.ToArray());

        var workflows = new List<BasicWorkflowItemDto>();

        foreach (var offer in offers.OrderByDescending(x => x.OfferDate))
        {
            cancellationToken.ThrowIfCancellationRequested();

            customerById.TryGetValue(offer.CustomerId, out var customer);
            var calculation = await calculationRepository.GetByOfferIdAsync(
                offer.Id,
                cancellationToken);

            projectByOfferId.TryGetValue(offer.Id, out var project);

            var linkedInvoices = new List<CustomerInvoiceDto>();

            if (invoicesByOfferId.TryGetValue(offer.Id, out var directInvoices))
                linkedInvoices.AddRange(directInvoices);

            if (project is not null
                && invoicesByProjectId.TryGetValue(project.Id, out var projectInvoices))
            {
                linkedInvoices.AddRange(
                    projectInvoices.Where(x =>
                        linkedInvoices.All(existing => existing.Id != x.Id)));
            }

            var invoice = linkedInvoices
                .OrderByDescending(x => x.InvoiceDate)
                .FirstOrDefault();

            var hasDunning = invoice is not null
                && dunningByInvoiceId.TryGetValue(invoice.Id, out var notices)
                && notices.Any(x => x.Status != DunningNoticeStatus.Cancelled);

            var hasCalculation = calculation is not null
                && calculation.Items.Count > 0;

            var issued = invoice?.Status is CustomerInvoiceStatus.Issued
                or CustomerInvoiceStatus.PartiallyPaid
                or CustomerInvoiceStatus.Paid;

            var hasPayment = invoice is not null
                && invoice.PaidAmount > 0m;

            var issues = new List<string>();

            if (customer is null)
                issues.Add("Kundenbezug fehlt.");

            if (offer.Status == OfferStatus.Accepted && !hasCalculation)
                issues.Add("Angenommenes Angebot besitzt keine Kalkulationspositionen.");

            if (offer.Status == OfferStatus.Accepted && project is null)
                issues.Add("Angenommenes Angebot besitzt noch kein Projekt.");

            if (project is not null && project.CustomerId != offer.CustomerId)
                issues.Add("Projekt und Angebot gehören zu unterschiedlichen Kunden.");

            if (invoice is not null && invoice.CustomerId != offer.CustomerId)
                issues.Add("Rechnung und Angebot gehören zu unterschiedlichen Kunden.");

            if (invoice is not null
                && invoice.SourceOfferId != offer.Id
                && invoice.SourceProjectId != project?.Id)
            {
                issues.Add("Rechnung besitzt keinen gültigen Quellbezug zum Angebot/Projekt.");
            }

            if (invoice is not null
                && invoice.IsOverdue
                && invoice.OpenAmount > 0m
                && !hasDunning)
            {
                issues.Add("Überfällige offene Rechnung besitzt noch keine Mahnung.");
            }

            var stage = BasicWorkflowRules.ResolveStage(
                offer.Status,
                hasCalculation,
                project?.Status,
                invoice?.Status,
                invoice?.OpenAmount ?? 0m,
                hasDunning);

            var completion = BasicWorkflowRules.CompletionPercent(
                hasOffer: true,
                hasCalculation,
                accepted: offer.Status == OfferStatus.Accepted,
                hasProject: project is not null,
                hasInvoice: invoice is not null,
                issued,
                hasPayment);

            workflows.Add(new BasicWorkflowItemDto(
                offer.Id,
                offer.OfferNumber,
                offer.CustomerId,
                customer?.DisplayName ?? "Unbekannter Kunde",
                offer.Title,
                offer.Status.ToString(),
                hasCalculation,
                project is not null,
                project?.ProjectNumber,
                invoice is not null,
                invoice?.InvoiceNumber,
                invoice?.Status.ToString(),
                invoice?.OpenAmount ?? 0m,
                hasPayment,
                hasDunning,
                stage,
                completion,
                issues.Count > 0,
                issues.Count == 0 ? null : string.Join(" ", issues)));
        }

        var orphans = new List<BasicWorkflowOrphanDto>();
        var offerIds = offers.Select(x => x.Id).ToHashSet();
        var projectIds = projects.Select(x => x.Id).ToHashSet();

        foreach (var project in projects.Where(x =>
                     x.SourceOfferId.HasValue
                     && !offerIds.Contains(x.SourceOfferId.Value)))
        {
            orphans.Add(new BasicWorkflowOrphanDto(
                "Projekt",
                project.Id,
                project.ProjectNumber,
                project.Title,
                "SourceOfferId verweist auf kein vorhandenes Angebot."));
        }

        foreach (var invoice in invoices)
        {
            if (invoice.SourceOfferId.HasValue
                && !offerIds.Contains(invoice.SourceOfferId.Value))
            {
                orphans.Add(new BasicWorkflowOrphanDto(
                    "Rechnung",
                    invoice.Id,
                    invoice.InvoiceNumber,
                    invoice.CustomerName,
                    "SourceOfferId verweist auf kein vorhandenes Angebot."));
            }

            if (invoice.SourceProjectId.HasValue
                && !projectIds.Contains(invoice.SourceProjectId.Value))
            {
                orphans.Add(new BasicWorkflowOrphanDto(
                    "Rechnung",
                    invoice.Id,
                    invoice.InvoiceNumber,
                    invoice.CustomerName,
                    "SourceProjectId verweist auf kein vorhandenes Projekt."));
            }
        }

        var invoiceIds = invoices.Select(x => x.Id).ToHashSet();

        foreach (var notice in dunnings.Where(x =>
                     !invoiceIds.Contains(x.CustomerInvoiceId)))
        {
            orphans.Add(new BasicWorkflowOrphanDto(
                "Mahnung",
                notice.Id,
                notice.NoticeNumber,
                notice.CustomerName,
                "CustomerInvoiceId verweist auf keine vorhandene Rechnung."));
        }

        return new BasicWorkflowAuditDto(
            DateTimeOffset.UtcNow,
            customers.Count,
            offers.Count,
            offers.Count(x => x.Status == OfferStatus.Accepted),
            workflows.Count,
            workflows.Count(x => x.Stage == "Abgeschlossen"),
            workflows.Count(x => x.HasIssue),
            orphans.Count,
            workflows,
            orphans);
    }
}
