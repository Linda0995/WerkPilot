using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WerkPilot.Infrastructure.Persistence.Migrations;

[DbContext(typeof(WerkPilotDbContext))]
[Migration("20260802110000_InitialCreate")]
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "customers",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CustomerNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Type = table.Column<int>(type: "integer", nullable: false),
                ContactPerson = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                VatId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                TaxProfile = table.Column<int>(type: "integer", nullable: false),
                Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                IsFavorite = table.Column<bool>(type: "boolean", nullable: false),
                billing_street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                billing_postal_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                billing_city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                billing_country_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                delivery_street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                delivery_postal_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                delivery_city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                delivery_country_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_customers", x => x.Id));

        migrationBuilder.CreateTable(
            name: "customer_contacts",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                Label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                IsPrimary = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_customer_contacts", x => x.Id);
                table.ForeignKey(
                    name: "FK_customer_contacts_customers_CustomerId",
                    column: x => x.CustomerId,
                    principalTable: "customers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_customer_contacts_CustomerId",
            table: "customer_contacts",
            column: "CustomerId");

        migrationBuilder.CreateIndex(
            name: "IX_customers_CustomerNumber",
            table: "customers",
            column: "CustomerNumber",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "customer_contacts");
        migrationBuilder.DropTable(name: "customers");
    }
}
