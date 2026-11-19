using GestorDeTareas.Data.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GestorDeTareas.Data.Migrations;

[DbContext(typeof(TasksDbContext))]
[Migration("20261118234500_InitialCreate")]
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Tasks",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                State = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                Responsible = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                DueDate = table.Column<DateOnly>(type: "TEXT", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Tasks", task => task.Id));

        migrationBuilder.CreateIndex(name: "IX_Tasks_Responsible", table: "Tasks", column: "Responsible");
        migrationBuilder.CreateIndex(name: "IX_Tasks_State", table: "Tasks", column: "State");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Tasks");
    }
}