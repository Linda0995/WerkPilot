using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WerkPilot.Infrastructure.Persistence.Migrations;

[DbContext(typeof(WerkPilotDbContext))]
[Migration("20260808210000_UserAbsences")]
public partial class UserAbsences : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "user_absences",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                UserDisplayName = table.Column<string>(
                    type: "character varying(200)", maxLength: 200, nullable: false),
                Type = table.Column<int>(type: "integer", nullable: false),
                StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                SubstituteUserId = table.Column<Guid>(type: "uuid", nullable: true),
                SubstituteDisplayName = table.Column<string>(
                    type: "character varying(200)", maxLength: 200, nullable: true),
                Note = table.Column<string>(
                    type: "character varying(4000)", maxLength: 4000, nullable: true),
                CreatedBy = table.Column<string>(
                    type: "character varying(150)", maxLength: 150, nullable: true),
                Status = table.Column<int>(type: "integer", nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_user_absences", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_user_absences_UserId_StartDate_EndDate",
            table: "user_absences",
            columns: new[] { "UserId", "StartDate", "EndDate" });

        migrationBuilder.CreateIndex(
            name: "IX_user_absences_SubstituteUserId",
            table: "user_absences",
            column: "SubstituteUserId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "user_absences");
    }
}
