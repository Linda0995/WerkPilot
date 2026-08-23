using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WerkPilot.Infrastructure.Persistence.Migrations;

[DbContext(typeof(WerkPilotDbContext))]
[Migration("20260804000000_ProjectTimeTracking")]
public partial class ProjectTimeTracking : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "time_entries",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                ProjectTaskId = table.Column<Guid>(type: "uuid", nullable: true),
                Description = table.Column<string>(
                    type: "character varying(1000)",
                    maxLength: 1000,
                    nullable: false),
                StartedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false),
                EndedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: true),
                CreatedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: true),
                IsDeleted = table.Column<bool>(
                    type: "boolean",
                    nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_time_entries", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_time_entries_ProjectId_StartedAtUtc",
            table: "time_entries",
            columns: new[] { "ProjectId", "StartedAtUtc" });

        migrationBuilder.CreateIndex(
            name: "IX_time_entries_UserId_EndedAtUtc",
            table: "time_entries",
            columns: new[] { "UserId", "EndedAtUtc" });
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "time_entries");
}
