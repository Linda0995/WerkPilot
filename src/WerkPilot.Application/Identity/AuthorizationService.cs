using WerkPilot.Domain.Identity;

namespace WerkPilot.Application.Identity;

public sealed class AuthorizationService(SessionContext session)
{
    public bool CanManageUsers() => session.Role is UserRole.Administrator;
    public bool CanEditCustomers() => session.Role is UserRole.Administrator or UserRole.Management or UserRole.Sales;
    public bool CanViewCustomers() => session.IsAuthenticated;
    public bool CanManageProduction() => session.Role is UserRole.Administrator or UserRole.Management or UserRole.Production;
    public bool CanEditOffers() => session.Role is UserRole.Administrator or UserRole.Management or UserRole.Sales;
    public bool CanViewOffers() => session.IsAuthenticated;

    public void Demand(Func<bool> permission, string message)
    {
        if (!session.IsAuthenticated)
            throw new UnauthorizedAccessException("Es ist keine aktive Anmeldung vorhanden.");
        if (!permission())
            throw new UnauthorizedAccessException(message);
    }
}
