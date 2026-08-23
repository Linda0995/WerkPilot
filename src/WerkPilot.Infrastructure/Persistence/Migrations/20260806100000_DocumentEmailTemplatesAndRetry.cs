using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WerkPilot.Infrastructure.Persistence.Migrations;

[DbContext(typeof(WerkPilotDbContext))]
[Migration("20260806100000_DocumentEmailTemplatesAndRetry")]
public partial class DocumentEmailTemplatesAndRetry : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "AttemptCount",
            table: "document_email_dispatches",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "LastAttemptAtUtc",
            table: "document_email_dispatches",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "NextRetryAtUtc",
            table: "document_email_dispatches",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "document_email_templates",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                DocumentType = table.Column<int>(type: "integer", nullable: false),
                Name = table.Column<string>(
                    type: "character varying(150)",
                    maxLength: 150,
                    nullable: false),
                SubjectTemplate = table.Column<string>(
                    type: "character varying(500)",
                    maxLength: 500,
                    nullable: false),
                BodyTemplate = table.Column<string>(
                    type: "character varying(10000)",
                    maxLength: 10000,
                    nullable: false),
                IsDefault = table.Column<bool>(type: "boolean", nullable: false),
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
                table.PrimaryKey("PK_document_email_templates", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_document_email_dispatches_NextRetryAtUtc",
            table: "document_email_dispatches",
            column: "NextRetryAtUtc");

        migrationBuilder.CreateIndex(
            name: "IX_document_email_templates_DocumentType_Name",
            table: "document_email_templates",
            columns: new[] { "DocumentType", "Name" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "document_email_templates");

        migrationBuilder.DropIndex(
            name: "IX_document_email_dispatches_NextRetryAtUtc",
            table: "document_email_dispatches");

        migrationBuilder.DropColumn(
            name: "AttemptCount",
            table: "document_email_dispatches");

        migrationBuilder.DropColumn(
            name: "LastAttemptAtUtc",
            table: "document_email_dispatches");

        migrationBuilder.DropColumn(
            name: "NextRetryAtUtc",
            table: "document_email_dispatches");
    }
}
