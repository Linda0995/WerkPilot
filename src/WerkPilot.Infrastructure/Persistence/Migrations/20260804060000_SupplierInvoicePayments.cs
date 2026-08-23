using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WerkPilot.Infrastructure.Persistence.Migrations;

[DbContext(typeof(WerkPilotDbContext))]
[Migration("20260804060000_SupplierInvoicePayments")]
public partial class SupplierInvoicePayments : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<decimal>(
            name: "CashDiscountPercent",
            table: "supplier_invoices",
            type: "numeric(5,2)",
            precision: 5,
            scale: 2,
            nullable: false,
            defaultValue: 0m);

        migrationBuilder.AddColumn<DateOnly>(
            name: "CashDiscountDueDate",
            table: "supplier_invoices",
            type: "date",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "supplier_invoice_payments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                SupplierInvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                Amount = table.Column<decimal>(
                    type: "numeric(18,2)",
                    precision: 18,
                    scale: 2,
                    nullable: false),
                PaymentDate = table.Column<DateOnly>(type: "date", nullable: false),
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
                    nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_supplier_invoice_payments", x => x.Id);
                table.ForeignKey(
                    name: "FK_supplier_invoice_payments_supplier_invoices_SupplierInvoiceId",
                    column: x => x.SupplierInvoiceId,
                    principalTable: "supplier_invoices",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_supplier_invoice_payments_SupplierInvoiceId_PaymentDate",
            table: "supplier_invoice_payments",
            columns: new[] { "SupplierInvoiceId", "PaymentDate" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "supplier_invoice_payments");

        migrationBuilder.DropColumn(
            name: "CashDiscountPercent",
            table: "supplier_invoices");

        migrationBuilder.DropColumn(
            name: "CashDiscountDueDate",
            table: "supplier_invoices");
    }
}
