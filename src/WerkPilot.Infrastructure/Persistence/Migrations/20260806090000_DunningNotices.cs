using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WerkPilot.Infrastructure.Persistence.Migrations;

[DbContext(typeof(WerkPilotDbContext))]
[Migration("20260806090000_DunningNotices")]
public partial class DunningNotices : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "dunning_notices",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                NoticeNumber = table.Column<string>(
                    type: "character varying(30)",
                    maxLength: 30,
                    nullable: false),
                CustomerInvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                CustomerInvoiceNumber = table.Column<string>(
                    type: "character varying(30)",
                    maxLength: 30,
                    nullable: false),
                CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                CustomerName = table.Column<string>(
                    type: "character varying(300)",
                    maxLength: 300,
                    nullable: false),
                NoticeDate = table.Column<DateOnly>(type: "date", nullable: false),
                PaymentDeadline = table.Column<DateOnly>(type: "date", nullable: false),
                Level = table.Column<int>(type: "integer", nullable: false),
                PrincipalAmount = table.Column<decimal>(
                    type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                FeeAmount = table.Column<decimal>(
                    type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                InterestAmount = table.Column<decimal>(
                    type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                AnnualInterestRatePercent = table.Column<decimal>(
                    type: "numeric(7,3)", precision: 7, scale: 3, nullable: false),
                OverdueDays = table.Column<int>(type: "integer", nullable: false),
                CreatedBy = table.Column<string>(
                    type: "character varying(150)", maxLength: 150, nullable: true),
                Status = table.Column<int>(type: "integer", nullable: false),
                IssuedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone", nullable: true),
                CreatedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_dunning_notices", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_dunning_notices_NoticeNumber",
            table: "dunning_notices",
            column: "NoticeNumber",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_dunning_notices_CustomerInvoiceId",
            table: "dunning_notices",
            column: "CustomerInvoiceId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "dunning_notices");
    }
}
