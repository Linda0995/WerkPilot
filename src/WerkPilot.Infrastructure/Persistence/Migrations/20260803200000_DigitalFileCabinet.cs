using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WerkPilot.Infrastructure.Persistence.Migrations;

[DbContext(typeof(WerkPilotDbContext))]
[Migration("20260803200000_DigitalFileCabinet")]
public partial class DigitalFileCabinet : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "document_folders",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(
                    type: "character varying(250)",
                    maxLength: 250,
                    nullable: false),
                OwnerType = table.Column<int>(type: "integer", nullable: false),
                OwnerId = table.Column<Guid>(type: "uuid", nullable: true),
                ParentFolderId = table.Column<Guid>(type: "uuid", nullable: true),
                CreatedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_document_folders", x => x.Id));

        migrationBuilder.CreateTable(
            name: "document_files",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                DisplayName = table.Column<string>(
                    type: "character varying(500)",
                    maxLength: 500,
                    nullable: false),
                StoredFileName = table.Column<string>(
                    type: "character varying(260)",
                    maxLength: 260,
                    nullable: false),
                RelativePath = table.Column<string>(
                    type: "character varying(1000)",
                    maxLength: 1000,
                    nullable: false),
                ContentType = table.Column<string>(
                    type: "character varying(150)",
                    maxLength: 150,
                    nullable: false),
                SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                OwnerType = table.Column<int>(type: "integer", nullable: false),
                OwnerId = table.Column<Guid>(type: "uuid", nullable: true),
                FolderId = table.Column<Guid>(type: "uuid", nullable: true),
                UploadedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_document_files", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_document_folders_OwnerType_OwnerId_ParentFolderId",
            table: "document_folders",
            columns: new[] { "OwnerType", "OwnerId", "ParentFolderId" });

        migrationBuilder.CreateIndex(
            name: "IX_document_files_OwnerType_OwnerId_FolderId",
            table: "document_files",
            columns: new[] { "OwnerType", "OwnerId", "FolderId" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "document_files");
        migrationBuilder.DropTable(name: "document_folders");
    }
}
