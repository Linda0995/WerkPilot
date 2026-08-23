using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WerkPilot.Infrastructure.Persistence.Migrations;

[DbContext(typeof(WerkPilotDbContext))]
[Migration("20260803140000_MaterialImportAndPurchaseList")]
public partial class MaterialImportAndPurchaseList : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "MaterialItemId",
            table: "calculation_items",
            type: "uuid",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_calculation_items_MaterialItemId",
            table: "calculation_items",
            column: "MaterialItemId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_calculation_items_MaterialItemId",
            table: "calculation_items");

        migrationBuilder.DropColumn(
            name: "MaterialItemId",
            table: "calculation_items");
    }
}
