using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WerkPilot.Infrastructure.Persistence.Migrations;

[DbContext(typeof(WerkPilotDbContext))]
[Migration("20260806093000_DocumentEmailDispatches")]
public partial class DocumentEmailDispatches : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "document_email_dispatches",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                DocumentType = table.Column<int>(type: "integer", nullable: false),
                DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                DocumentNumber = table.Column<string>(
                    type: "character varying(50)", maxLength: 50, nullable: false),
                Recipient = table.Column<string>(
                    type: "character varying(320)", maxLength: 320, nullable: false),
                Subject = table.Column<string>(
                    type: "character varying(500)", maxLength: 500, nullable: false),
                Body = table.Column<string>(
                    type: "character varying(10000)", maxLength: 10000, nullable: false),
                AttachmentFileName = table.Column<string>(
                    type: "character varying(300)", maxLength: 300, nullable: false),
                CreatedBy = table.Column<string>(
                    type: "character varying(150)", maxLength: 150, nullable: true),
                Status = table.Column<int>(type: "integer", nullable: false),
                SentAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone", nullable: true),
                ErrorMessage = table.Column<string>(
                    type: "character varying(4000)", maxLength: 4000, nullable: true),
                CreatedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_document_email_dispatches", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_document_email_dispatches_DocumentType_DocumentId",
            table: "document_email_dispatches",
            columns: new[] { "DocumentType", "DocumentId" });

        migrationBuilder.CreateIndex(
            name: "IX_document_email_dispatches_CreatedAtUtc",
            table: "document_email_dispatches",
            column: "CreatedAtUtc");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "document_email_dispatches");
    }
}
