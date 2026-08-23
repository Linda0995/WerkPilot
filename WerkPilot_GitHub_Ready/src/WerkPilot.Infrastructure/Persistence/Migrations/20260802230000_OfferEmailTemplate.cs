using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WerkPilot.Infrastructure.Persistence.Migrations;

[DbContext(typeof(WerkPilotDbContext))]
[Migration("20260802230000_OfferEmailTemplate")]
public partial class OfferEmailTemplate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "OfferEmailSubjectTemplate",
            table: "company_profiles",
            type: "character varying(500)",
            maxLength: 500,
            nullable: false,
            defaultValue: "Angebot {OfferNumber} – {OfferTitle}");

        migrationBuilder.AddColumn<string>(
            name: "OfferEmailBodyTemplate",
            table: "company_profiles",
            type: "character varying(5000)",
            maxLength: 5000,
            nullable: false,
            defaultValue:
                "Sehr geehrte Damen und Herren,\n\nanbei erhalten Sie unser Angebot {OfferNumber}.\n\nMit freundlichen Grüßen\n{CompanyName}");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "OfferEmailSubjectTemplate", table: "company_profiles");
        migrationBuilder.DropColumn(name: "OfferEmailBodyTemplate", table: "company_profiles");
    }
}
