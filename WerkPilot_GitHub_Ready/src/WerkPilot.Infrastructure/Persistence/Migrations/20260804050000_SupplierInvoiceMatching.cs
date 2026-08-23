using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WerkPilot.Infrastructure.Persistence.Migrations;

[DbContext(typeof(WerkPilotDbContext))]
[Migration("20260804050000_SupplierInvoiceMatching")]
public partial class SupplierInvoiceMatching : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "supplier_invoices",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                InvoiceNumber = table.Column<string>(
                    type: "character varying(100)",
                    maxLength: 100,
                    nullable: false),
                SupplierOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                SupplierName = table.Column<string>(
                    type: "character varying(250)",
                    maxLength: 250,
                    nullable: false),
                InvoiceDate = table.Column<DateOnly>(type: "date", nullable: false),
                DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                CreatedBy = table.Column<string>(
                    type: "character varying(150)",
                    maxLength: 150,
                    nullable: true),
                Status = table.Column<int>(type: "integer", nullable: false),
                ReviewNote = table.Column<string>(
                    type: "character varying(2000)",
                    maxLength: 2000,
                    nullable: true),
                ApprovedBy = table.Column<string>(
                    type: "character varying(150)",
                    maxLength: 150,
                    nullable: true),
                ApprovedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: true),
                PaidAtUtc = table.Column<DateTimeOffset>(
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
                table.PrimaryKey("PK_supplier_invoices", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "supplier_invoice_lines",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                SupplierInvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                SupplierOrderLineId = table.Column<Guid>(type: "uuid", nullable: false),
                MaterialItemId = table.Column<Guid>(type: "uuid", nullable: false),
                ArticleNumber = table.Column<string>(
                    type: "character varying(100)",
                    maxLength: 100,
                    nullable: false),
                Description = table.Column<string>(
                    type: "character varying(500)",
                    maxLength: 500,
                    nullable: false),
                InvoicedQuantity = table.Column<decimal>(
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
                table.PrimaryKey("PK_supplier_invoice_lines", x => x.Id);
                table.ForeignKey(
                    name: "FK_supplier_invoice_lines_supplier_invoices_SupplierInvoiceId",
                    column: x => x.SupplierInvoiceId,
                    principalTable: "supplier_invoices",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_supplier_invoices_SupplierName_InvoiceNumber",
            table: "supplier_invoices",
            columns: new[] { "SupplierName", "InvoiceNumber" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_supplier_invoice_lines_SupplierInvoiceId_SupplierOrderLineId",
            table: "supplier_invoice_lines",
            columns: new[] { "SupplierInvoiceId", "SupplierOrderLineId" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "supplier_invoice_lines");
        migrationBuilder.DropTable(name: "supplier_invoices");
    }
}
