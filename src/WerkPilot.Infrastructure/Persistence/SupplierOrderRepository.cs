using Microsoft.EntityFrameworkCore;
using WerkPilot.Application.Purchasing;
using WerkPilot.Domain.Purchasing;

namespace WerkPilot.Infrastructure.Persistence;

public sealed class SupplierOrderRepository(WerkPilotDbContext dbContext)
    : ISupplierOrderRepository
{
    public async Task<IReadOnlyList<SupplierOrder>> GetAllAsync(
        CancellationToken cancellationToken) =>
        await dbContext.SupplierOrders
            .Include(x => x.Lines)
            .OrderByDescending(x => x.OrderDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public Task<SupplierOrder?> GetAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        dbContext.SupplierOrders
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<string> GetNextNumberAsync(
        int year,
        CancellationToken cancellationToken)
    {
        var prefix = $"BE-{year}-";

        var numbers = await dbContext.SupplierOrders
            .IgnoreQueryFilters()
            .Where(x => x.OrderNumber.StartsWith(prefix))
            .Select(x => x.OrderNumber)
            .ToListAsync(cancellationToken);

        var maximum = numbers
            .Select(x => int.TryParse(x[prefix.Length..], out var value) ? value : 0)
            .DefaultIfEmpty()
            .Max();

        return $"{prefix}{maximum + 1:0000}";
    }

    public Task AddAsync(
        SupplierOrder order,
        CancellationToken cancellationToken) =>
        dbContext.SupplierOrders.AddAsync(order, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
