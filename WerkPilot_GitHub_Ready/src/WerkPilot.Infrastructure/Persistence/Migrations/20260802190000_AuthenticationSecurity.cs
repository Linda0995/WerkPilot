using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WerkPilot.Infrastructure.Persistence.Migrations;

[DbContext(typeof(WerkPilotDbContext))]
[Migration("20260802190000_AuthenticationSecurity")]
public partial class AuthenticationSecurity : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>("PasswordHash", "app_users", "character varying(200)", maxLength: 200, nullable: true);
        migrationBuilder.AddColumn<string>("PasswordSalt", "app_users", "character varying(100)", maxLength: 100, nullable: true);
        migrationBuilder.AddColumn<int>("FailedLoginCount", "app_users", "integer", nullable: false, defaultValue: 0);
        migrationBuilder.AddColumn<DateTimeOffset>("LockedUntilUtc", "app_users", "timestamp with time zone", nullable: true);
        migrationBuilder.AddColumn<DateTimeOffset>("LastLoginAtUtc", "app_users", "timestamp with time zone", nullable: true);
        migrationBuilder.AddColumn<bool>("MustChangePassword", "app_users", "boolean", nullable: false, defaultValue: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn("PasswordHash", "app_users");
        migrationBuilder.DropColumn("PasswordSalt", "app_users");
        migrationBuilder.DropColumn("FailedLoginCount", "app_users");
        migrationBuilder.DropColumn("LockedUntilUtc", "app_users");
        migrationBuilder.DropColumn("LastLoginAtUtc", "app_users");
        migrationBuilder.DropColumn("MustChangePassword", "app_users");
    }
}
