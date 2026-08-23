using WerkPilot.Domain.Identity;

namespace WerkPilot.Application.Identity;

public interface IUserAbsenceRepository
{
    Task<IReadOnlyList<UserAbsence>> GetAllAsync(CancellationToken cancellationToken);
    Task<UserAbsence?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(UserAbsence absence, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
