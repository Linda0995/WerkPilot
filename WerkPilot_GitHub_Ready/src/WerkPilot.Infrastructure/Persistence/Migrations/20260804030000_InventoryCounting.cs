using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WerkPilot.Infrastructure.Persistence.Migrations;

[DbContext(typeof(WerkPilotDbContext))]
[Migration("20260804030000_InventoryCounting")]
public partial class InventoryCounting : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "inventory_counts",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CountNumber = table.Column<string>(
                    type: "character varying(30)",
                    maxLength: 30,
                    nullable: false),
                Title = table.Column<string>(
                    type: "character varying(300)",
                    maxLength: 300,
                    nullable: false),
                CountDate = table.Column<DateOnly>(type: "date", nullable: false),
                StorageLocation = table.Column<string>(
                    type: "character varying(150)",
                    maxLength: 150,
                    nullable: true),
                CreatedBy = table.Column<string>(
                    type: "character varying(150)",
                    maxLength: 150,
                    nullable: true),
                Status = table.Column<int>(type: "integer", nullable: false),
                PostedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: true),
                PostedBy = table.Column<string>(
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
                table.PrimaryKey("PK_inventory_counts", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "inventory_count_lines",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                InventoryCountId = table.Column<Guid>(type: "uuid", nullable: false),
                InventoryItemId = table.Column<Guid>(type: "uuid", nullable: false),
                ExpectedQuantity = table.Column<decimal>(
                    type: "numeric(18,3)",
                    precision: 18,
                    scale: 3,
                    nullable: false),
                CountedQuantity = table.Column<decimal>(
                    type: "numeric(18,3)",
                    precision: 18,
                    scale: 3,
                    nullable: true),
                Note = table.Column<string>(
                    type: "character varying(1000)",
                    maxLength: 1000,
                    nullable: true),
                CountedBy = table.Column<string>(
                    type: "character varying(150)",
                    maxLength: 150,
                    nullable: true),
                CountedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_inventory_count_lines", x => x.Id);
                table.ForeignKey(
                    name: "FK_inventory_count_lines_inventory_counts_InventoryCountId",
                    column: x => x.InventoryCountId,
                    principalTable: "inventory_counts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_inventory_counts_CountNumber",
            table: "inventory_counts",
            column: "CountNumber",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_inventory_count_lines_InventoryCountId_InventoryItemId",
            table: "inventory_count_lines",
            columns: new[] { "InventoryCountId", "InventoryItemId" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "inventory_count_lines");
        migrationBuilder.DropTable(name: "inventory_counts");
    }
}
