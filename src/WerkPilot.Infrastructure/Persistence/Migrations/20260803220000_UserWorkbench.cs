using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WerkPilot.Infrastructure.Persistence.Migrations;

[DbContext(typeof(WerkPilotDbContext))]
[Migration("20260803220000_UserWorkbench")]
public partial class UserWorkbench : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "workbench_items",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                ItemType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                Number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                Subtitle = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                IsFavorite = table.Column<bool>(type: "boolean", nullable: false),
                LastOpenedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_workbench_items", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_workbench_items_UserId_ItemType_EntityId",
            table: "workbench_items",
            columns: new[] { "UserId", "ItemType", "EntityId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_workbench_items_UserId_IsFavorite_LastOpenedAtUtc",
            table: "workbench_items",
            columns: new[] { "UserId", "IsFavorite", "LastOpenedAtUtc" });
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "workbench_items");
}
