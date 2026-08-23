using WerkPilot.Application.Common;

namespace WerkPilot.Application.Customers;

public sealed class CustomerValidationException(ValidationResult validationResult)
    : Exception(validationResult.ToDisplayText())
{
    public ValidationResult ValidationResult { get; } = validationResult;
}
