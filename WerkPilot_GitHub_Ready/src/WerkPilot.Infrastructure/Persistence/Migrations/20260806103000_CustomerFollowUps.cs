using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WerkPilot.Infrastructure.Persistence.Migrations;

[DbContext(typeof(WerkPilotDbContext))]
[Migration("20260806103000_CustomerFollowUps")]
public partial class CustomerFollowUps : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "customer_follow_ups",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                CustomerNumber = table.Column<string>(
                    type: "character varying(50)",
                    maxLength: 50,
                    nullable: false),
                CustomerName = table.Column<string>(
                    type: "character varying(300)",
                    maxLength: 300,
                    nullable: false),
                Title = table.Column<string>(
                    type: "character varying(500)",
                    maxLength: 500,
                    nullable: false),
                Notes = table.Column<string>(
                    type: "character varying(4000)",
                    maxLength: 4000,
                    nullable: true),
                DueAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false),
                Priority = table.Column<int>(type: "integer", nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                AssignedUserId = table.Column<Guid>(type: "uuid", nullable: true),
                AssignedTo = table.Column<string>(
                    type: "character varying(200)",
                    maxLength: 200,
                    nullable: true),
                CreatedBy = table.Column<string>(
                    type: "character varying(150)",
                    maxLength: 150,
                    nullable: true),
                CompletedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: true),
                CompletionNote = table.Column<string>(
                    type: "character varying(4000)",
                    maxLength: 4000,
                    nullable: true),
                CreatedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_customer_follow_ups", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_customer_follow_ups_CustomerId",
            table: "customer_follow_ups",
            column: "CustomerId");

        migrationBuilder.CreateIndex(
            name: "IX_customer_follow_ups_Status_DueAtUtc",
            table: "customer_follow_ups",
            columns: new[] { "Status", "DueAtUtc" });

        migrationBuilder.CreateIndex(
            name: "IX_customer_follow_ups_AssignedUserId",
            table: "customer_follow_ups",
            column: "AssignedUserId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "customer_follow_ups");
    }
}
