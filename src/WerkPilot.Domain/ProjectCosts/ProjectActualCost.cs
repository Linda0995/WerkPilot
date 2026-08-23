using WerkPilot.Domain.Common;

namespace WerkPilot.Domain.ProjectCosts;

public sealed class ProjectActualCost : Entity
{
    private ProjectActualCost() { }

    public ProjectActualCost(
        Guid projectId,
        ProjectActualCostType costType,
        string description,
        decimal amountNet,
        DateOnly costDate,
        string? reference)
    {
        if (projectId == Guid.Empty)
            throw new ArgumentException("Projekt erforderlich.", nameof(projectId));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Beschreibung erforderlich.", nameof(description));
        if (amountNet < 0)
            throw new ArgumentOutOfRangeException(nameof(amountNet));

        ProjectId = projectId;
        CostType = costType;
        Description = description.Trim();
        AmountNet = amountNet;
        CostDate = costDate;
        Reference = Clean(reference);
    }

    public Guid ProjectId { get; private set; }
    public ProjectActualCostType CostType { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal AmountNet { get; private set; }
    public DateOnly CostDate { get; private set; }
    public string? Reference { get; private set; }

    public void Update(
        ProjectActualCostType costType,
        string description,
        decimal amountNet,
        DateOnly costDate,
        string? reference)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Beschreibung erforderlich.", nameof(description));
        if (amountNet < 0)
            throw new ArgumentOutOfRangeException(nameof(amountNet));

        CostType = costType;
        Description = description.Trim();
        AmountNet = amountNet;
        CostDate = costDate;
        Reference = Clean(reference);
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
