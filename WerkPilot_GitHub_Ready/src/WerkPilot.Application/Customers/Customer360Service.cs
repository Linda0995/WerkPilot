using WerkPilot.Application.Crm;
using WerkPilot.Application.Documents;
using WerkPilot.Application.Offers;
using WerkPilot.Application.Projects;
using WerkPilot.Domain.Documents;
using WerkPilot.Domain.Offers;
using WerkPilot.Domain.Projects;

namespace WerkPilot.Application.Customers;

public sealed class Customer360Service(
    CustomerService customerService,
    OfferService offerService,
    ProjectService projectService,
    CustomerInteractionService interactionService,
    DocumentService documentService)
{
    public async Task<Customer360Dto> GetAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var customers = await customerService.SearchAsync(
            null,
            includeDeleted: true,
            cancellationToken);

        var customer = customers.SingleOrDefault(x => x.Id == customerId)
            ?? throw new InvalidOperationException("Kunde wurde nicht gefunden.");

        var offers = (await offerService.GetAllAsync(cancellationToken))
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.OfferDate)
            .ToArray();

        var projects = (await projectService.GetAllAsync(cancellationToken))
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.PlannedStart)
            .ToArray();

        var interactions = await interactionService.GetForCustomerAsync(
            customerId,
            cancellationToken);

        var openFollowUps = interactions
            .Where(x => x.FollowUpDate.HasValue && !x.FollowUpCompleted)
            .OrderBy(x => x.FollowUpDate)
            .ToArray();

        var directDocuments = await documentService.GetFilesAsync(
            DocumentOwnerType.Customer,
            customerId,
            includeDeleted: false,
            cancellationToken);

        var projectDocuments = new List<DocumentFileDto>();

        foreach (var project in projects)
        {
            projectDocuments.AddRange(await documentService.GetFilesAsync(
                DocumentOwnerType.Project,
                project.Id,
                includeDeleted: false,
                cancellationToken));
        }

        var documents = directDocuments
            .Concat(projectDocuments)
            .OrderByDescending(x => x.UploadedAtUtc)
            .ToArray();

        var openOfferVolume = offers
            .Where(x => x.Status is OfferStatus.Draft or OfferStatus.Sent)
            .Sum(x => x.NetTotal);

        var activeProjects = projects.Count(x =>
            x.Status is ProjectStatus.Planned or ProjectStatus.Active or ProjectStatus.OnHold);

        return new Customer360Dto(
            customer,
            offers,
            projects,
            interactions,
            documents,
            openFollowUps,
            openOfferVolume,
            activeProjects,
            openFollowUps.Length);
    }
}
