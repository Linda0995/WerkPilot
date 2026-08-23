using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace WerkPilot.Infrastructure.Persistence.Migrations;
[DbContext(typeof(WerkPilotDbContext))]
[Migration("20260803220000_NotificationCenter")]
public partial class NotificationCenter : Migration
{
 protected override void Up(MigrationBuilder migrationBuilder)
 {
  migrationBuilder.CreateTable(name:"notification_read_states",columns:table=>new
  {
   Id=table.Column<Guid>(type:"uuid",nullable:false), UserId=table.Column<Guid>(type:"uuid",nullable:false),
   NotificationKey=table.Column<string>(type:"character varying(300)",maxLength:300,nullable:false),
   ReadAtUtc=table.Column<DateTimeOffset>(type:"timestamp with time zone",nullable:false),
   CreatedAtUtc=table.Column<DateTimeOffset>(type:"timestamp with time zone",nullable:false),
   UpdatedAtUtc=table.Column<DateTimeOffset>(type:"timestamp with time zone",nullable:true), IsDeleted=table.Column<bool>(type:"boolean",nullable:false)
  },constraints:table=>table.PrimaryKey("PK_notification_read_states",x=>x.Id));
  migrationBuilder.CreateIndex(name:"IX_notification_read_states_UserId_NotificationKey",table:"notification_read_states",columns:new[]{"UserId","NotificationKey"},unique:true);
 }
 protected override void Down(MigrationBuilder migrationBuilder)=>migrationBuilder.DropTable(name:"notification_read_states");
}
