using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WerkPilot.Infrastructure.Persistence.Migrations;

[DbContext(typeof(WerkPilotDbContext))]
[Migration("20260808190000_ProjectTaskUserAssignments")]
public partial class ProjectTaskUserAssignments : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "AssignedUserId",
            table: "project_tasks",
            type: "uuid",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_project_tasks_AssignedUserId",
            table: "project_tasks",
            column: "AssignedUserId");

        migrationBuilder.Sql(
            "UPDATE project_tasks AS pt " +
            "SET \"AssignedUserId\" = u.\"Id\" " +
            "FROM app_users AS u " +
            "WHERE pt.\"AssignedUserId\" IS NULL " +
            "AND pt.\"AssignedTo\" IS NOT NULL " +
            "AND u.\"IsDeleted\" = FALSE " +
            "AND LOWER(TRIM(pt.\"AssignedTo\")) = LOWER(TRIM(u.\"DisplayName\"));");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_project_tasks_AssignedUserId",
            table: "project_tasks");

        migrationBuilder.DropColumn(
            name: "AssignedUserId",
            table: "project_tasks");
    }
}
