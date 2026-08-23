using WerkPilot.Infrastructure.Security;
namespace WerkPilot.UnitTests;
public sealed class Pbkdf2PasswordHasherTests
{
    [Fact] public void Hash_AndVerify_WithCorrectPassword_Succeeds()
    {
        var hasher = new Pbkdf2PasswordHasher();
        var result = hasher.Hash("SehrSicher!2026");
        Assert.True(hasher.Verify("SehrSicher!2026", result.Hash, result.Salt));
        Assert.False(hasher.Verify("FalschesPasswort", result.Hash, result.Salt));
    }
    [Fact] public void Hash_WithShortPassword_Throws() =>
        Assert.Throws<ArgumentException>(() => new Pbkdf2PasswordHasher().Hash("kurz"));
}
