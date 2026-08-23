using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WerkPilot.Infrastructure.Persistence.Migrations;

[DbContext(typeof(WerkPilotDbContext))]
[Migration("20260803100000_OfferCalculationFoundation")]
public partial class OfferCalculationFoundation : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "offer_calculations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                OfferId = table.Column<Guid>(type: "uuid", nullable: false),
                ProfitTargetPercent = table.Column<decimal>(
                    type: "numeric(7,2)",
                    precision: 7,
                    scale: 2,
                    nullable: false),
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
                table.PrimaryKey("PK_offer_calculations", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "calculation_items",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CalculationId = table.Column<Guid>(type: "uuid", nullable: false),
                PositionNumber = table.Column<int>(type: "integer", nullable: false),
                CostType = table.Column<int>(type: "integer", nullable: false),
                Description = table.Column<string>(
                    type: "character varying(1000)",
                    maxLength: 1000,
                    nullable: false),
                Quantity = table.Column<decimal>(
                    type: "numeric(18,3)",
                    precision: 18,
                    scale: 3,
                    nullable: false),
                UnitCost = table.Column<decimal>(
                    type: "numeric(18,2)",
                    precision: 18,
                    scale: 2,
                    nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_calculation_items", x => x.Id);
                table.ForeignKey(
                    name: "FK_calculation_items_offer_calculations_CalculationId",
                    column: x => x.CalculationId,
                    principalTable: "offer_calculations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_offer_calculations_OfferId",
            table: "offer_calculations",
            column: "OfferId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_calculation_items_CalculationId",
            table: "calculation_items",
            column: "CalculationId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "calculation_items");
        migrationBuilder.DropTable(name: "offer_calculations");
    }
}
