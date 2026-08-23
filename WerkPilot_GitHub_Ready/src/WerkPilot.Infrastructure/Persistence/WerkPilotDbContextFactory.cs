using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WerkPilot.Infrastructure.Persistence;

public sealed class WerkPilotDbContextFactory : IDesignTimeDbContextFactory<WerkPilotDbContext>
{
    public WerkPilotDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("WERKPILOT_CONNECTION_STRING");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Für EF-Core-Werkzeuge muss WERKPILOT_CONNECTION_STRING gesetzt sein.");
        }

        var options = new DbContextOptionsBuilder<WerkPilotDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new WerkPilotDbContext(options);
    }
}
