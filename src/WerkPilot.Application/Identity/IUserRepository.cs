using WerkPilot.Domain.Identity;
namespace WerkPilot.Application.Identity;
public interface IUserRepository
{
    Task<IReadOnlyList<AppUser>> GetAllAsync(CancellationToken cancellationToken);
    Task<AppUser?> GetAsync(Guid id,CancellationToken cancellationToken);
    Task<AppUser?> FindByUserNameAsync(string userName,CancellationToken cancellationToken);
    Task AddAsync(AppUser user,CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
