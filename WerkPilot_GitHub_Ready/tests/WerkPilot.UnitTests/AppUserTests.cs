using WerkPilot.Domain.Identity;
namespace WerkPilot.UnitTests;
public sealed class AppUserTests
{
 [Fact] public void Constructor_NormalizesUserName(){var u=new AppUser(" Admin ","Administrator",UserRole.Administrator);Assert.Equal("admin",u.UserName);Assert.True(u.IsActive);}
 [Fact] public void ActivateAndDeactivate_ChangeState(){var u=new AppUser("user","Benutzer",UserRole.ReadOnly);u.Deactivate();Assert.False(u.IsActive);u.Activate();Assert.True(u.IsActive);}
 [Fact] public void ChangeRole_UpdatesRole(){var u=new AppUser("sales","Vertrieb",UserRole.Sales);u.ChangeRole(UserRole.Management);Assert.Equal(UserRole.Management,u.Role);}
}
