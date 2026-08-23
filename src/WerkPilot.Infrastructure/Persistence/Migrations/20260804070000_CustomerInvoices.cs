using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WerkPilot.Infrastructure.Persistence.Migrations;

[DbContext(typeof(WerkPilotDbContext))]
[Migration("20260804070000_CustomerInvoices")]
public partial class CustomerInvoices : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "customer_invoices",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                InvoiceNumber = table.Column<string>(
                    type: "character varying(30)",
                    maxLength: 30,
                    nullable: false),
                CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                CustomerName = table.Column<string>(
                    type: "character varying(300)",
                    maxLength: 300,
                    nullable: false),
                InvoiceDate = table.Column<DateOnly>(type: "date", nullable: false),
                DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                SourceOfferId = table.Column<Guid>(type: "uuid", nullable: true),
                SourceProjectId = table.Column<Guid>(type: "uuid", nullable: true),
                CreatedBy = table.Column<string>(
                    type: "character varying(150)",
                    maxLength: 150,
                    nullable: true),
                Status = table.Column<int>(type: "integer", nullable: false),
                DunningLevel = table.Column<int>(type: "integer", nullable: false),
                LastDunningDate = table.Column<DateOnly>(type: "date", nullable: true),
                IssuedAtUtc = table.Column<DateTimeOffset>(
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
                table.PrimaryKey("PK_customer_invoices", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "customer_invoice_lines",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CustomerInvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                Description = table.Column<string>(
                    type: "character varying(1000)",
                    maxLength: 1000,
                    nullable: false),
                Quantity = table.Column<decimal>(
                    type: "numeric(18,3)",
                    precision: 18,
                    scale: 3,
                    nullable: false),
                Unit = table.Column<string>(
                    type: "character varying(30)",
                    maxLength: 30,
                    nullable: false),
                UnitPriceNet = table.Column<decimal>(
                    type: "numeric(18,4)",
                    precision: 18,
                    scale: 4,
                    nullable: false),
                VatRatePercent = table.Column<decimal>(
                    type: "numeric(5,2)",
                    precision: 5,
                    scale: 2,
                    nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_customer_invoice_lines", x => x.Id);
                table.ForeignKey(
                    name: "FK_customer_invoice_lines_customer_invoices_CustomerInvoiceId",
                    column: x => x.CustomerInvoiceId,
                    principalTable: "customer_invoices",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "customer_invoice_payments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CustomerInvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
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
                table.PrimaryKey("PK_customer_invoice_payments", x => x.Id);
                table.ForeignKey(
                    name: "FK_customer_invoice_payments_customer_invoices_CustomerInvoiceId",
                    column: x => x.CustomerInvoiceId,
                    principalTable: "customer_invoices",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_customer_invoices_InvoiceNumber",
            table: "customer_invoices",
            column: "InvoiceNumber",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_customer_invoice_lines_CustomerInvoiceId",
            table: "customer_invoice_lines",
            column: "CustomerInvoiceId");

        migrationBuilder.CreateIndex(
            name: "IX_customer_invoice_payments_CustomerInvoiceId_PaymentDate",
            table: "customer_invoice_payments",
            columns: new[] { "CustomerInvoiceId", "PaymentDate" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "customer_invoice_lines");
        migrationBuilder.DropTable(name: "customer_invoice_payments");
        migrationBuilder.DropTable(name: "customer_invoices");
    }
}
