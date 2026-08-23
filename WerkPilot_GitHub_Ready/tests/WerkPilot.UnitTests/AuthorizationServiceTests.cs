using WerkPilot.Application.Identity;
using WerkPilot.Domain.Identity;

namespace WerkPilot.UnitTests;

public sealed class AuthorizationServiceTests
{
    [Theory]
    [InlineData(UserRole.Administrator, true)]
    [InlineData(UserRole.Management, true)]
    [InlineData(UserRole.Sales, true)]
    [InlineData(UserRole.Production, false)]
    [InlineData(UserRole.ReadOnly, false)]
    public void CanEditCustomers_ReflectsRole(UserRole role, bool expected)
    {
        var session = new SessionContext();
        session.SignIn(Guid.NewGuid(), "Test", role);
        var authorization = new AuthorizationService(session);

        Assert.Equal(expected, authorization.CanEditCustomers());
    }
}
