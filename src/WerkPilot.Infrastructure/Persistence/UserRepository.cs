using Microsoft.EntityFrameworkCore;
using WerkPilot.Application.Identity;
using WerkPilot.Domain.Identity;
namespace WerkPilot.Infrastructure.Persistence;
public sealed class UserRepository(WerkPilotDbContext dbContext) : IUserRepository
{
    public async Task<IReadOnlyList<AppUser>> GetAllAsync(CancellationToken ct)=>await dbContext.Users.IgnoreQueryFilters().AsNoTracking().OrderBy(x=>x.DisplayName).ToListAsync(ct);
    public Task<AppUser?> GetAsync(Guid id,CancellationToken ct)=>dbContext.Users.IgnoreQueryFilters().SingleOrDefaultAsync(x=>x.Id==id,ct);
    public Task<AppUser?> FindByUserNameAsync(string name,CancellationToken ct)=>dbContext.Users.IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(x=>x.UserName==name.Trim().ToLower(),ct);
    public Task AddAsync(AppUser user,CancellationToken ct)=>dbContext.Users.AddAsync(user,ct).AsTask();
    public Task SaveChangesAsync(CancellationToken ct)=>dbContext.SaveChangesAsync(ct);
}
