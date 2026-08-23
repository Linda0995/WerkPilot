using Microsoft.EntityFrameworkCore;
using WerkPilot.Application.Billing;
using WerkPilot.Domain.Billing;

namespace WerkPilot.Infrastructure.Persistence;

public sealed class DunningNoticeRepository(WerkPilotDbContext dbContext)
    : IDunningNoticeRepository
{
    public async Task<IReadOnlyList<DunningNotice>> GetAllAsync(
        CancellationToken cancellationToken) =>
        await dbContext.DunningNotices
            .AsNoTracking()
            .OrderByDescending(x => x.NoticeDate)
            .ToListAsync(cancellationToken);

    public Task<DunningNotice?> GetAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        dbContext.DunningNotices.SingleOrDefaultAsync(
            x => x.Id == id,
            cancellationToken);

    public async Task<string> GetNextNumberAsync(
        int year,
        CancellationToken cancellationToken)
    {
        var prefix = $"MA-{year}-";
        var numbers = await dbContext.DunningNotices
            .IgnoreQueryFilters()
            .Where(x => x.NoticeNumber.StartsWith(prefix))
            .Select(x => x.NoticeNumber)
            .ToListAsync(cancellationToken);

        var maximum = numbers
            .Select(x => int.TryParse(x[prefix.Length..], out var value) ? value : 0)
            .DefaultIfEmpty()
            .Max();

        return $"{prefix}{maximum + 1:0000}";
    }

    public Task AddAsync(
        DunningNotice notice,
        CancellationToken cancellationToken) =>
        dbContext.DunningNotices.AddAsync(notice, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
