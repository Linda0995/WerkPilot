using WerkPilot.Application.Identity;

namespace WerkPilot.UnitTests;

public sealed class PasswordPolicyTests
{
    [Fact]
    public void Validate_WithStrongMatchingPassword_Succeeds()
    {
        PasswordPolicy.Validate("Sicheres!Kennwort2026", "Sicheres!Kennwort2026");
    }

    [Theory]
    [InlineData("kurz")]
    [InlineData("nurkleinbuchstaben123")]
    [InlineData("NURBUCHSTABEN!")]
    [InlineData("OhneSonderzeichen123")]
    public void Validate_WithWeakPassword_Throws(string password)
    {
        Assert.Throws<UserValidationException>(() =>
            PasswordPolicy.Validate(password, password));
    }

    [Fact]
    public void Validate_WithDifferentConfirmation_Throws()
    {
        Assert.Throws<UserValidationException>(() =>
            PasswordPolicy.Validate("Sicheres!Kennwort2026", "Anderes!Kennwort2026"));
    }
}
