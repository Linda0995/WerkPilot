using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WerkPilot.Infrastructure.Persistence.Migrations;

[DbContext(typeof(WerkPilotDbContext))]
[Migration("20260802130000_CrmCompletion")]
public partial class CrmCompletion : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_customers_DisplayName",
            table: "customers",
            column: "DisplayName");

        migrationBuilder.CreateIndex(
            name: "IX_customers_VatId",
            table: "customers",
            column: "VatId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_customers_DisplayName", table: "customers");
        migrationBuilder.DropIndex(name: "IX_customers_VatId", table: "customers");
    }
}
