using Serilog;

namespace WerkPilot.Desktop.Services;

public static class UiErrorFormatter
{
    public static string Format(
        Exception exception,
        string operation)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var errorId = Guid.NewGuid()
            .ToString("N")[..8]
            .ToUpperInvariant();

        Log.Error(
            exception,
            "UI operation failed. ErrorId={ErrorId}, Operation={Operation}",
            errorId,
            operation);

        if (IsUserFacing(exception))
        {
            var message = Normalize(exception.Message);

            return string.IsNullOrWhiteSpace(operation)
                ? message
                : $"{operation}: {message}";
        }

        return string.IsNullOrWhiteSpace(operation)
            ? $"Der Vorgang konnte nicht abgeschlossen werden. Fehler-ID: {errorId}"
            : $"{operation}. Bitte erneut versuchen. Fehler-ID: {errorId}";
    }

    public static string Startup(
        Exception exception,
        string component)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var errorId = Guid.NewGuid()
            .ToString("N")[..8]
            .ToUpperInvariant();

        Log.Fatal(
            exception,
            "Startup component failed. ErrorId={ErrorId}, Component={Component}",
            errorId,
            component);

        return $"{component} konnte nicht gestartet werden. "
            + $"Prüfe Konfiguration und Verbindung. Fehler-ID: {errorId}";
    }

    private static bool IsUserFacing(Exception exception)
    {
        if (exception is ArgumentException
            or UnauthorizedAccessException)
        {
            return true;
        }

        var typeName = exception.GetType().Name;

        return typeName is
            "UserValidationException"
            or "CustomerValidationException"
            or "CustomerDuplicateException";
    }

    private static string Normalize(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return "Die Eingaben konnten nicht verarbeitet werden.";

        var normalized = message
            .Replace('\r', ' ')
            .Replace('\n', ' ')
            .Trim();

        while (normalized.Contains("  ", StringComparison.Ordinal))
            normalized = normalized.Replace("  ", " ", StringComparison.Ordinal);

        return normalized.Length <= 320
            ? normalized
            : normalized[..317] + "...";
    }
}
