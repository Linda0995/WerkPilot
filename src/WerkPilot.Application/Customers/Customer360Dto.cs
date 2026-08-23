using WerkPilot.Application.Crm;
using WerkPilot.Application.Documents;
using WerkPilot.Application.Offers;
using WerkPilot.Application.Projects;

namespace WerkPilot.Application.Customers;

public sealed record Customer360Dto(
    CustomerDto Customer,
    IReadOnlyList<OfferDto> Offers,
    IReadOnlyList<ProjectDto> Projects,
    IReadOnlyList<CustomerInteractionDto> Interactions,
    IReadOnlyList<DocumentFileDto> Documents,
    IReadOnlyList<CustomerInteractionDto> OpenFollowUps,
    decimal OpenOfferVolumeNet,
    int ActiveProjectCount,
    int OpenFollowUpCount);
