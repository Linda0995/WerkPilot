using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WerkPilot.Infrastructure.Persistence.Migrations;

[DbContext(typeof(WerkPilotDbContext))]
[Migration("20260806080000_CustomerCreditNotes")]
public partial class CustomerCreditNotes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<decimal>(
            name: "CreditedAmount",
            table: "customer_invoices",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: false,
            defaultValue: 0m);

        migrationBuilder.CreateTable(
            name: "customer_credit_notes",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CreditNoteNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                CustomerInvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                CustomerInvoiceNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                CustomerName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                CreditNoteDate = table.Column<DateOnly>(type: "date", nullable: false),
                Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                CreatedBy = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                Status = table.Column<int>(type: "integer", nullable: false),
                IssuedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                AppliedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_customer_credit_notes", x => x.Id));

        migrationBuilder.CreateTable(
            name: "customer_credit_note_lines",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CustomerCreditNoteId = table.Column<Guid>(type: "uuid", nullable: false),
                SourceInvoiceLineId = table.Column<Guid>(type: "uuid", nullable: true),
                Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                Quantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                Unit = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                UnitPriceNet = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                VatRatePercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_customer_credit_note_lines", x => x.Id);
                table.ForeignKey(
                    name: "FK_customer_credit_note_lines_customer_credit_notes_CustomerCreditNoteId",
                    column: x => x.CustomerCreditNoteId,
                    principalTable: "customer_credit_notes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_customer_credit_notes_CreditNoteNumber",
            table: "customer_credit_notes",
            column: "CreditNoteNumber",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_customer_credit_notes_CustomerInvoiceId",
            table: "customer_credit_notes",
            column: "CustomerInvoiceId");

        migrationBuilder.CreateIndex(
            name: "IX_customer_credit_note_lines_CustomerCreditNoteId",
            table: "customer_credit_note_lines",
            column: "CustomerCreditNoteId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "customer_credit_note_lines");
        migrationBuilder.DropTable(name: "customer_credit_notes");
        migrationBuilder.DropColumn(name: "CreditedAmount", table: "customer_invoices");
    }
}
