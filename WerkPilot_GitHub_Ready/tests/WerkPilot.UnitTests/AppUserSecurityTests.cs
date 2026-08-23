using WerkPilot.Domain.Identity;
namespace WerkPilot.UnitTests;
public sealed class AppUserSecurityTests
{
    [Fact] public void FiveFailedLogins_LockUser()
    {
        var user = new AppUser("test", "Test", UserRole.ReadOnly);
        var now = DateTimeOffset.UtcNow;
        for (var i = 0; i < 5; i++) user.RecordFailedLogin(now, 5, TimeSpan.FromMinutes(15));
        Assert.True(user.IsLocked(now.AddMinutes(1)));
    }
    [Fact] public void SuccessfulLogin_ResetsLockState()
    {
        var user = new AppUser("test", "Test", UserRole.ReadOnly);
        var now = DateTimeOffset.UtcNow;
        user.RecordFailedLogin(now, 1, TimeSpan.FromMinutes(15));
        user.RecordSuccessfulLogin(now.AddMinutes(1));
        Assert.False(user.IsLocked(now.AddMinutes(2)));
        Assert.Equal(now.AddMinutes(1), user.LastLoginAtUtc);
    }
}
