using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WerkPilot.Infrastructure.Persistence.Migrations;

[DbContext(typeof(WerkPilotDbContext))]
[Migration("20260804040000_SupplierOrders")]
public partial class SupplierOrders : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "supplier_orders",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                OrderNumber = table.Column<string>(
                    type: "character varying(30)",
                    maxLength: 30,
                    nullable: false),
                SupplierName = table.Column<string>(
                    type: "character varying(250)",
                    maxLength: 250,
                    nullable: false),
                SupplierReference = table.Column<string>(
                    type: "character varying(200)",
                    maxLength: 200,
                    nullable: true),
                OrderDate = table.Column<DateOnly>(type: "date", nullable: false),
                ExpectedDeliveryDate = table.Column<DateOnly>(type: "date", nullable: true),
                CreatedBy = table.Column<string>(
                    type: "character varying(150)",
                    maxLength: 150,
                    nullable: true),
                Status = table.Column<int>(type: "integer", nullable: false),
                OrderedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: true),
                ReceivedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
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
                table.PrimaryKey("PK_supplier_orders", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "supplier_order_lines",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                SupplierOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                MaterialItemId = table.Column<Guid>(type: "uuid", nullable: false),
                ArticleNumber = table.Column<string>(
                    type: "character varying(100)",
                    maxLength: 100,
                    nullable: false),
                Description = table.Column<string>(
                    type: "character varying(500)",
                    maxLength: 500,
                    nullable: false),
                Unit = table.Column<string>(
                    type: "character varying(30)",
                    maxLength: 30,
                    nullable: false),
                OrderedQuantity = table.Column<decimal>(
                    type: "numeric(18,3)",
                    precision: 18,
                    scale: 3,
                    nullable: false),
                ReceivedQuantity = table.Column<decimal>(
                    type: "numeric(18,3)",
                    precision: 18,
                    scale: 3,
                    nullable: false),
                UnitPriceNet = table.Column<decimal>(
                    type: "numeric(18,4)",
                    precision: 18,
                    scale: 4,
                    nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_supplier_order_lines", x => x.Id);
                table.ForeignKey(
                    name: "FK_supplier_order_lines_supplier_orders_SupplierOrderId",
                    column: x => x.SupplierOrderId,
                    principalTable: "supplier_orders",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_supplier_orders_OrderNumber",
            table: "supplier_orders",
            column: "OrderNumber",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_supplier_order_lines_SupplierOrderId_MaterialItemId",
            table: "supplier_order_lines",
            columns: new[] { "SupplierOrderId", "MaterialItemId" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "supplier_order_lines");
        migrationBuilder.DropTable(name: "supplier_orders");
    }
}
