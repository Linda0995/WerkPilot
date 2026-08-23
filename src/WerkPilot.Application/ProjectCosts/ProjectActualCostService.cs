using WerkPilot.Application.Auditing;
using WerkPilot.Domain.ProjectCosts;

namespace WerkPilot.Application.ProjectCosts;

public sealed class ProjectActualCostService(
    IProjectActualCostRepository repository,
    IAuditTrail auditTrail)
{
    public async Task<IReadOnlyList<ProjectActualCostDto>> GetForProjectAsync(
        Guid projectId,
        CancellationToken cancellationToken = default) =>
        (await repository.GetForProjectAsync(projectId, cancellationToken))
            .OrderByDescending(x => x.CostDate)
            .Select(Map)
            .ToArray();

    public async Task<ProjectActualCostDto> CreateAsync(
        Guid projectId,
        ProjectActualCostType costType,
        string description,
        decimal amountNet,
        DateOnly costDate,
        string? reference,
        CancellationToken cancellationToken = default)
    {
        var cost = new ProjectActualCost(
            projectId, costType, description, amountNet, costDate, reference);

        await repository.AddAsync(cost, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        await auditTrail.WriteAsync(
            "ProjectActualCost",
            cost.Id,
            "Created",
            $"Ist-Kosten „{cost.Description}“ wurden mit {cost.AmountNet:N2} € erfasst.",
            cancellationToken);

        return Map(cost);
    }

    public async Task UpdateAsync(
        Guid id,
        ProjectActualCostType costType,
        string description,
        decimal amountNet,
        DateOnly costDate,
        string? reference,
        CancellationToken cancellationToken = default)
    {
        var cost = await repository.GetAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Ist-Kostenbuchung wurde nicht gefunden.");

        cost.Update(costType, description, amountNet, costDate, reference);
        await repository.SaveChangesAsync(cancellationToken);
    }

    private static ProjectActualCostDto Map(ProjectActualCost x) => new(
        x.Id, x.ProjectId, x.CostType, x.Description, x.AmountNet, x.CostDate, x.Reference);
}
