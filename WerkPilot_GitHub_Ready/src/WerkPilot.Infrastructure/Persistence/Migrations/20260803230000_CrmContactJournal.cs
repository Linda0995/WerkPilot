using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WerkPilot.Infrastructure.Persistence.Migrations;

[DbContext(typeof(WerkPilotDbContext))]
[Migration("20260803230000_CrmContactJournal")]
public partial class CrmContactJournal : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "LastContactAtUtc",
            table: "customers",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "customer_interactions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                InteractionType = table.Column<int>(type: "integer", nullable: false),
                Subject = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                Notes = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                ContactPerson = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                CreatedBy = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                FollowUpDate = table.Column<DateOnly>(type: "date", nullable: true),
                FollowUpOwner = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                FollowUpCompleted = table.Column<bool>(type: "boolean", nullable: false),
                FollowUpCompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_customer_interactions", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_customer_interactions_CustomerId_OccurredAtUtc",
            table: "customer_interactions",
            columns: new[] { "CustomerId", "OccurredAtUtc" });

        migrationBuilder.CreateIndex(
            name: "IX_customer_interactions_FollowUpCompleted_FollowUpDate",
            table: "customer_interactions",
            columns: new[] { "FollowUpCompleted", "FollowUpDate" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "customer_interactions");
        migrationBuilder.DropColumn(name: "LastContactAtUtc", table: "customers");
    }
}
