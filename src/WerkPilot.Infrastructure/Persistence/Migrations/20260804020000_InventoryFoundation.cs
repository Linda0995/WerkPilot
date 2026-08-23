using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WerkPilot.Infrastructure.Persistence.Migrations;

[DbContext(typeof(WerkPilotDbContext))]
[Migration("20260804020000_InventoryFoundation")]
public partial class InventoryFoundation : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "inventory_items",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                MaterialItemId = table.Column<Guid>(type: "uuid", nullable: false),
                StorageLocation = table.Column<string>(
                    type: "character varying(150)",
                    maxLength: 150,
                    nullable: true),
                QuantityOnHand = table.Column<decimal>(
                    type: "numeric(18,3)",
                    precision: 18,
                    scale: 3,
                    nullable: false),
                ReservedQuantity = table.Column<decimal>(
                    type: "numeric(18,3)",
                    precision: 18,
                    scale: 3,
                    nullable: false),
                MinimumStock = table.Column<decimal>(
                    type: "numeric(18,3)",
                    precision: 18,
                    scale: 3,
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
                table.PrimaryKey("PK_inventory_items", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "inventory_movements",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                InventoryItemId = table.Column<Guid>(type: "uuid", nullable: false),
                MovementType = table.Column<int>(type: "integer", nullable: false),
                Quantity = table.Column<decimal>(
                    type: "numeric(18,3)",
                    precision: 18,
                    scale: 3,
                    nullable: false),
                Reason = table.Column<string>(
                    type: "character varying(500)",
                    maxLength: 500,
                    nullable: false),
                OccurredAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false),
                ProjectId = table.Column<Guid>(type: "uuid", nullable: true),
                Reference = table.Column<string>(
                    type: "character varying(200)",
                    maxLength: 200,
                    nullable: true),
                CreatedBy = table.Column<string>(
                    type: "character varying(150)",
                    maxLength: 150,
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
                table.PrimaryKey("PK_inventory_movements", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_inventory_items_MaterialItemId",
            table: "inventory_items",
            column: "MaterialItemId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_inventory_movements_InventoryItemId_OccurredAtUtc",
            table: "inventory_movements",
            columns: new[] { "InventoryItemId", "OccurredAtUtc" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "inventory_movements");
        migrationBuilder.DropTable(name: "inventory_items");
    }
}
