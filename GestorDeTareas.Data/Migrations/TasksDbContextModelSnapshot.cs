using GestorDeTareas.Data.Models;
using GestorDeTareas.Data.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace GestorDeTareas.Data.Migrations;

[DbContext(typeof(TasksDbContext))]
public partial class TasksDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "10.0.14")
            .HasAnnotation("Relational:MaxIdentifierLength", 64);

        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.Property(task => task.Id)
                .ValueGeneratedOnAdd()
                .HasColumnType("INTEGER")
                .HasAnnotation("Sqlite:Autoincrement", true);
            entity.Property(task => task.Description)
                .IsRequired()
                .HasMaxLength(2000)
                .HasColumnType("TEXT");
            entity.Property(task => task.DueDate)
                .HasColumnType("TEXT");
            entity.Property(task => task.Responsible)
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnType("TEXT");
            entity.Property(task => task.State)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnType("TEXT");
            entity.Property(task => task.Title)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("TEXT");
            entity.HasKey(task => task.Id);
            entity.HasIndex(task => task.Responsible);
            entity.HasIndex(task => task.State);
            entity.ToTable("Tasks");
        });
    }
}