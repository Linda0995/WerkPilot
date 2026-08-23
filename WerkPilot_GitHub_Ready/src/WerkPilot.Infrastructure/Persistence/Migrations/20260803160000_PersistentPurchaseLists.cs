using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WerkPilot.Infrastructure.Persistence.Migrations;

[DbContext(typeof(WerkPilotDbContext))]
[Migration("20260803160000_PersistentPurchaseLists")]
public partial class PersistentPurchaseLists : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "purchase_lists",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                PurchaseListNumber = table.Column<string>(
                    type: "character varying(30)",
                    maxLength: 30,
                    nullable: false),
                OfferId = table.Column<Guid>(type: "uuid", nullable: false),
                Title = table.Column<string>(
                    type: "character varying(300)",
                    maxLength: 300,
                    nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
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
                table.PrimaryKey("PK_purchase_lists", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "purchase_list_items",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                PurchaseListId = table.Column<Guid>(type: "uuid", nullable: false),
                PositionNumber = table.Column<int>(type: "integer", nullable: false),
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
                RequiredQuantity = table.Column<decimal>(
                    type: "numeric(18,3)",
                    precision: 18,
                    scale: 3,
                    nullable: false),
                PurchasePrice = table.Column<decimal>(
                    type: "numeric(18,4)",
                    precision: 18,
                    scale: 4,
                    nullable: false),
                Supplier = table.Column<string>(
                    type: "character varying(200)",
                    maxLength: 200,
                    nullable: true),
                IsOrdered = table.Column<bool>(type: "boolean", nullable: false),
                OrderedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: true),
                OrderNote = table.Column<string>(
                    type: "character varying(1000)",
                    maxLength: 1000,
                    nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_purchase_list_items", x => x.Id);
                table.ForeignKey(
                    name: "FK_purchase_list_items_purchase_lists_PurchaseListId",
                    column: x => x.PurchaseListId,
                    principalTable: "purchase_lists",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_purchase_lists_OfferId",
            table: "purchase_lists",
            column: "OfferId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_purchase_lists_PurchaseListNumber",
            table: "purchase_lists",
            column: "PurchaseListNumber",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_purchase_list_items_PurchaseListId",
            table: "purchase_list_items",
            column: "PurchaseListId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "purchase_list_items");
        migrationBuilder.DropTable(name: "purchase_lists");
    }
}
