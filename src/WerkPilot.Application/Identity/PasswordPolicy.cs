namespace WerkPilot.Application.Identity;

public static class PasswordPolicy
{
    public static void Validate(string password, string confirmation)
    {
        if (!string.Equals(password, confirmation, StringComparison.Ordinal))
            throw new UserValidationException("Die Kennwortbestätigung stimmt nicht überein.");

        if (password.Length < 12)
            throw new UserValidationException("Das Kennwort muss mindestens 12 Zeichen enthalten.");

        if (!password.Any(char.IsUpper) ||
            !password.Any(char.IsLower) ||
            !password.Any(char.IsDigit) ||
            !password.Any(ch => !char.IsLetterOrDigit(ch)))
            throw new UserValidationException(
                "Das Kennwort muss Groß- und Kleinbuchstaben, eine Zahl und ein Sonderzeichen enthalten.");
    }
}
