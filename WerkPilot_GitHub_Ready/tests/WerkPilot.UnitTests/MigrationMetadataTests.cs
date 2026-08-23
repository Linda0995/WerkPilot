using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WerkPilot.Infrastructure.Persistence;
using WerkPilot.Infrastructure.Persistence.Migrations;

namespace WerkPilot.UnitTests;

public sealed class MigrationMetadataTests
{
    [Fact]
    public void InitialMigration_HasExpectedMetadata()
    {
        var type = typeof(InitialCreate);
        var migration = Assert.Single(type.GetCustomAttributes(typeof(MigrationAttribute), false));
        var dbContext = Assert.Single(type.GetCustomAttributes(typeof(DbContextAttribute), false));

        Assert.Equal("20260802110000_InitialCreate", ((MigrationAttribute)migration).Id);
        Assert.Equal(typeof(WerkPilotDbContext), ((DbContextAttribute)dbContext).ContextType);
    }

    [Fact]
    public void CrmCompletionMigration_HasExpectedMetadata()
    {
        var type = typeof(CrmCompletion);
        var migration = Assert.Single(type.GetCustomAttributes(typeof(MigrationAttribute), false));
        Assert.Equal("20260802130000_CrmCompletion", ((MigrationAttribute)migration).Id);
    }

    [Fact]
    public void CrmAuditTrailMigration_HasExpectedMetadata()
    {
        var type = typeof(CrmAuditTrail);
        var migration = Assert.Single(type.GetCustomAttributes(typeof(MigrationAttribute), false));
        Assert.Equal("20260802150000_CrmAuditTrail", ((MigrationAttribute)migration).Id);
    }

    [Fact]
    public void AuthenticationSecurityMigration_HasExpectedMetadata()
    {
        var type = typeof(AuthenticationSecurity);
        var migration = Assert.Single(type.GetCustomAttributes(typeof(MigrationAttribute), false));
        Assert.Equal("20260802190000_AuthenticationSecurity", ((MigrationAttribute)migration).Id);
    }
}
