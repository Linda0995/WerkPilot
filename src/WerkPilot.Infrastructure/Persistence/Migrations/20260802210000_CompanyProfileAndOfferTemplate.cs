using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WerkPilot.Infrastructure.Persistence.Migrations;

[DbContext(typeof(WerkPilotDbContext))]
[Migration("20260802210000_CompanyProfileAndOfferTemplate")]
public partial class CompanyProfileAndOfferTemplate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "company_profiles",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CompanyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                PostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                CountryCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                VatId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                Website = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                OfferIntroText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                OfferClosingText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                CurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_company_profiles", x => x.Id));
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "company_profiles");
    }
}
