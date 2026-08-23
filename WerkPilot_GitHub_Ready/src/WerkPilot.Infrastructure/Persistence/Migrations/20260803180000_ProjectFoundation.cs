using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WerkPilot.Infrastructure.Persistence.Migrations;

[DbContext(typeof(WerkPilotDbContext))]
[Migration("20260803180000_ProjectFoundation")]
public partial class ProjectFoundation : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "projects",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ProjectNumber = table.Column<string>(
                    type: "character varying(30)",
                    maxLength: 30,
                    nullable: false),
                CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                SourceOfferId = table.Column<Guid>(type: "uuid", nullable: true),
                Title = table.Column<string>(
                    type: "character varying(300)",
                    maxLength: 300,
                    nullable: false),
                Description = table.Column<string>(
                    type: "character varying(4000)",
                    maxLength: 4000,
                    nullable: true),
                ProjectManager = table.Column<string>(
                    type: "character varying(150)",
                    maxLength: 150,
                    nullable: true),
                PlannedStart = table.Column<DateOnly>(type: "date", nullable: false),
                PlannedEnd = table.Column<DateOnly>(type: "date", nullable: true),
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
                table.PrimaryKey("PK_projects", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "project_tasks",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                PositionNumber = table.Column<int>(type: "integer", nullable: false),
                Title = table.Column<string>(
                    type: "character varying(500)",
                    maxLength: 500,
                    nullable: false),
                AssignedTo = table.Column<string>(
                    type: "character varying(150)",
                    maxLength: 150,
                    nullable: true),
                DueDate = table.Column<DateOnly>(type: "date", nullable: true),
                Status = table.Column<int>(type: "integer", nullable: false),
                CompletedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_project_tasks", x => x.Id);
                table.ForeignKey(
                    name: "FK_project_tasks_projects_ProjectId",
                    column: x => x.ProjectId,
                    principalTable: "projects",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_projects_CustomerId",
            table: "projects",
            column: "CustomerId");

        migrationBuilder.CreateIndex(
            name: "IX_projects_ProjectNumber",
            table: "projects",
            column: "ProjectNumber",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_projects_SourceOfferId",
            table: "projects",
            column: "SourceOfferId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_project_tasks_ProjectId",
            table: "project_tasks",
            column: "ProjectId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "project_tasks");
        migrationBuilder.DropTable(name: "projects");
    }
}
