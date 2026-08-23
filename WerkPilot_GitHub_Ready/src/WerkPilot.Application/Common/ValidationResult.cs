namespace WerkPilot.Application.Common;

public sealed class ValidationResult
{
    private readonly List<ValidationError> _errors = [];

    public bool IsValid => _errors.Count == 0;
    public IReadOnlyList<ValidationError> Errors => _errors;

    public void Add(string propertyName, string message) =>
        _errors.Add(new ValidationError(propertyName, message));

    public string ToDisplayText() =>
        string.Join(Environment.NewLine, _errors.Select(x => $"• {x.Message}"));
}
