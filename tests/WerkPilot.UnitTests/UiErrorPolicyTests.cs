namespace WerkPilot.UnitTests;

public sealed class UiErrorPolicyTests
{
    [Fact]
    public void TechnicalException_ShouldNotExposeTechnicalMessage()
    {
        var exception = new Exception(
            "Host=database.internal;Password=super-secret");

        var isUserFacing =
            exception is ArgumentException
            or UnauthorizedAccessException
            || exception.GetType().Name is
                "UserValidationException"
                or "CustomerValidationException"
                or "CustomerDuplicateException";

        Assert.False(isUserFacing);
    }

    [Fact]
    public void ArgumentException_IsExpectedUserFacingError()
    {
        Exception exception =
            new ArgumentException("Fälligkeit ist ungültig.");

        Assert.IsType<ArgumentException>(exception);
        Assert.Equal(
            "Fälligkeit ist ungültig.",
            exception.Message);
    }
}
