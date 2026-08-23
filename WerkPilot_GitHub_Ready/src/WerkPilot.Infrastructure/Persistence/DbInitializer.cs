using Microsoft.EntityFrameworkCore;
using WerkPilot.Application.Identity;
using WerkPilot.Domain.Customers;
using WerkPilot.Domain.Identity;
using WerkPilot.Infrastructure.Security;

namespace WerkPilot.Infrastructure.Persistence;

public static class DbInitializer
{
    private const string InitialAdminPasswordVariable =
        "WERKPILOT_ADMIN_INITIAL_PASSWORD";

    private const string SeedDemoDataVariable =
        "WERKPILOT_SEED_DEMO_DATA";

    public static async Task InitializeAsync(
        WerkPilotDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);

        await EnsureAdministratorAsync(
            dbContext,
            cancellationToken);

        if (!ShouldSeedDemoData())
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        await EnsureDemoCustomerAsync(
            dbContext,
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureAdministratorAsync(
        WerkPilotDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (await dbContext.Users
                .IgnoreQueryFilters()
                .AnyAsync(cancellationToken))
        {
            return;
        }

        var plainPassword =
            Environment.GetEnvironmentVariable(
                InitialAdminPasswordVariable);

        if (string.IsNullOrWhiteSpace(plainPassword))
        {
            throw new InvalidOperationException(
                $"Erstinstallation: Um den Administrator sicher anzulegen, "
                + $"muss die Umgebungsvariable {InitialAdminPasswordVariable} "
                + "mit einem temporären Initialkennwort gesetzt sein.");
        }

        PasswordPolicy.Validate(
            plainPassword,
            plainPassword);

        var admin = new AppUser(
            "admin",
            "WerkPilot Administrator",
            UserRole.Administrator);

        var initialPassword =
            new Pbkdf2PasswordHasher().Hash(plainPassword);

        admin.SetPassword(
            initialPassword.Hash,
            initialPassword.Salt,
            mustChangePassword: true);

        dbContext.Users.Add(admin);

        // Das Klartextkennwort wird weder gespeichert noch protokolliert.
    }

    private static bool ShouldSeedDemoData()
    {
        var value =
            Environment.GetEnvironmentVariable(
                SeedDemoDataVariable);

        return bool.TryParse(value, out var enabled)
            && enabled;
    }

    private static async Task EnsureDemoCustomerAsync(
        WerkPilotDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (await dbContext.Customers
                .AnyAsync(cancellationToken))
        {
            return;
        }

        var customer = new Customer(
            "K-2026-0001",
            "Musterbetrieb GmbH",
            CustomerType.Company);

        customer.UpdatePrimaryContact(
            "Max Mustermann",
            "office@musterbetrieb.at",
            "+43 123 456789");

        customer.SetAddresses(
            new Address(
                "Werkstraße 1",
                "8010",
                "Graz",
                "AT"),
            new Address(
                "Lagerweg 2",
                "8073",
                "Feldkirchen",
                "AT"));

        customer.UpdateTax(
            "ATU12345678",
            TaxProfile.Domestic);

        customer.UpdateNotes(
            "Demo-Kunde für WerkPilot 0.12.24 RC.");

        customer.SetFavorite(true);

        customer.AddContact(
            "Einkauf",
            "einkauf@musterbetrieb.at",
            "+43 123 456780",
            true);

        dbContext.Customers.Add(customer);
    }
}
