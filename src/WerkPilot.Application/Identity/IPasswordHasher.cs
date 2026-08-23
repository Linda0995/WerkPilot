namespace WerkPilot.Application.Identity;

public interface IPasswordHasher
{
    PasswordHashResult Hash(string password);
    bool Verify(string password, string hash, string salt);
}

public sealed record PasswordHashResult(string Hash, string Salt);
