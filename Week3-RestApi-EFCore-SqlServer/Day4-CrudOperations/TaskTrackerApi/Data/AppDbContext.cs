using Microsoft.EntityFrameworkCore;
using TaskTrackerApi.Models;

namespace TaskTrackerApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<Tag> Tags => Set<Tag>();

    // Many-to-many between TaskItem and Tag needs explicit configuration
    // in EF Core so it creates the TaskTags join table correctly.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<TaskItem>()
        .HasMany(t => t.Tags)
        .WithMany(tag => tag.Tasks)
        .UsingEntity(j => j.ToTable("TaskTags"));

    modelBuilder.Entity<TaskItem>()
        .HasOne(t => t.AssignedToUser)
        .WithMany()
        .HasForeignKey(t => t.AssignedToUserId)
        .OnDelete(DeleteBehavior.NoAction);

    // Explicitly specifying precision (18 total digits) and scale (2
    // decimal places) for Budget, instead of relying on EF Core's
    // default — this silences the earlier warning and guarantees the
    // exact column type SQL Server uses, consistent with the DECIMAL
    // choice already documented in the Day 2 schema design.
    modelBuilder.Entity<Project>()
        .Property(p => p.Budget)
        .HasPrecision(18, 2);
}
}