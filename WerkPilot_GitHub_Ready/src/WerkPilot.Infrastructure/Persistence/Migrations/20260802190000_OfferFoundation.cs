using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WerkPilot.Infrastructure.Persistence.Migrations;

[DbContext(typeof(WerkPilotDbContext))]
[Migration("20260802190000_OfferFoundation")]
public partial class OfferFoundation : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "offers",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                OfferNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                Title = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                OfferDate = table.Column<DateOnly>(type: "date", nullable: false),
                ValidUntil = table.Column<DateOnly>(type: "date", nullable: false),
                TaxRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_offers", x => x.Id));

        migrationBuilder.CreateTable(
            name: "offer_positions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                OfferId = table.Column<Guid>(type: "uuid", nullable: false),
                PositionNumber = table.Column<int>(type: "integer", nullable: false),
                Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                Quantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                UnitPriceNet = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_offer_positions", x => x.Id);
                table.ForeignKey(
                    name: "FK_offer_positions_offers_OfferId",
                    column: x => x.OfferId,
                    principalTable: "offers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(name: "IX_offers_CustomerId", table: "offers", column: "CustomerId");
        migrationBuilder.CreateIndex(name: "IX_offers_OfferNumber", table: "offers", column: "OfferNumber", unique: true);
        migrationBuilder.CreateIndex(name: "IX_offer_positions_OfferId", table: "offer_positions", column: "OfferId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "offer_positions");
        migrationBuilder.DropTable(name: "offers");
    }
}
