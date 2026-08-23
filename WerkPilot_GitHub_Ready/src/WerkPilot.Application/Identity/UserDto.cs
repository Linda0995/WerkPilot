using WerkPilot.Domain.Identity;
namespace WerkPilot.Application.Identity;
public sealed record UserDto(Guid Id,string UserName,string DisplayName,UserRole Role,bool IsActive);
