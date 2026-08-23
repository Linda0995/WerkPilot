using Microsoft.EntityFrameworkCore;
using WerkPilot.Application.Identity;
using WerkPilot.Domain.Identity;

namespace WerkPilot.Infrastructure.Persistence;

public sealed class UserAbsenceRepository(WerkPilotDbContext dbContext)
    : IUserAbsenceRepository
{
    public async Task<IReadOnlyList<UserAbsence>> GetAllAsync(
        CancellationToken cancellationToken) =>
        await dbContext.UserAbsences
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public Task<UserAbsence?> GetAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        dbContext.UserAbsences.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task AddAsync(
        UserAbsence absence,
        CancellationToken cancellationToken) =>
        dbContext.UserAbsences.AddAsync(absence, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
