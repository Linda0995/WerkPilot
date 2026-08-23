using Microsoft.EntityFrameworkCore;
using WerkPilot.Application.Settings;
using WerkPilot.Domain.Settings;

namespace WerkPilot.Infrastructure.Persistence;

public sealed class CompanyProfileRepository(WerkPilotDbContext dbContext)
    : ICompanyProfileRepository
{
    public Task<CompanyProfile?> GetAsync(CancellationToken cancellationToken) =>
        dbContext.CompanyProfiles.SingleOrDefaultAsync(cancellationToken);

    public Task AddAsync(CompanyProfile profile, CancellationToken cancellationToken) =>
        dbContext.CompanyProfiles.AddAsync(profile, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
