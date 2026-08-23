using WerkPilot.Domain.Settings;

namespace WerkPilot.Application.Settings;

public interface ICompanyProfileRepository
{
    Task<CompanyProfile?> GetAsync(CancellationToken cancellationToken);
    Task AddAsync(CompanyProfile profile, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
