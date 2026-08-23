using WerkPilot.Domain.Identity;
namespace WerkPilot.Application.Identity;
public sealed record CreateUserRequest(string UserName,string DisplayName,UserRole Role);
