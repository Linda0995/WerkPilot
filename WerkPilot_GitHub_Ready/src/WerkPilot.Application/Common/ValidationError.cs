namespace WerkPilot.Application.Common;

public sealed record ValidationError(string PropertyName, string Message);
