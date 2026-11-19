using GestorDeTareas.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GestorDeTareas.Data.Persistence;

public class TasksDbContext(DbContextOptions<TasksDbContext> options) : DbContext(options)
{
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var task = modelBuilder.Entity<TaskItem>();
        task.ToTable("Tasks");
        task.HasKey(item => item.Id);
        task.Property(item => item.Title).HasMaxLength(200).IsRequired();
        task.Property(item => item.Description).HasMaxLength(2000).IsRequired();
        task.Property(item => item.State).HasConversion<string>().HasMaxLength(20).IsRequired();
        task.Property(item => item.Responsible).HasMaxLength(120).IsRequired();
        task.Property(item => item.DueDate).IsRequired();
        task.HasIndex(item => item.State);
        task.HasIndex(item => item.Responsible);
    }
}