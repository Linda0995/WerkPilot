using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace WerkPilot.Infrastructure.Persistence.Migrations;
[DbContext(typeof(WerkPilotDbContext))]
[Migration("20260802170000_IdentityFoundation")]
public partial class IdentityFoundation : Migration
{
 protected override void Up(MigrationBuilder migrationBuilder)
 {
  migrationBuilder.CreateTable(name:"app_users",columns:table=>new
  {
   Id=table.Column<Guid>(type:"uuid",nullable:false),
   UserName=table.Column<string>(type:"character varying(100)",maxLength:100,nullable:false),
   DisplayName=table.Column<string>(type:"character varying(150)",maxLength:150,nullable:false),
   Role=table.Column<int>(type:"integer",nullable:false), IsActive=table.Column<bool>(type:"boolean",nullable:false),
   CreatedAtUtc=table.Column<DateTimeOffset>(type:"timestamp with time zone",nullable:false),
   UpdatedAtUtc=table.Column<DateTimeOffset>(type:"timestamp with time zone",nullable:true),
   IsDeleted=table.Column<bool>(type:"boolean",nullable:false)
  },constraints:table=>table.PrimaryKey("PK_app_users",x=>x.Id));
  migrationBuilder.CreateIndex(name:"IX_app_users_UserName",table:"app_users",column:"UserName",unique:true);
 }
 protected override void Down(MigrationBuilder migrationBuilder)=>migrationBuilder.DropTable(name:"app_users");
}
