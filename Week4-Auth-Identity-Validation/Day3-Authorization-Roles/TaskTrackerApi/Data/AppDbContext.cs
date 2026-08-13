using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskTrackerApi.Models;

namespace TaskTrackerApi.Data;

// Inheriting from IdentityDbContext<IdentityUser> instead of plain
// DbContext brings in the full Identity schema (Users, Roles, UserRoles,
// and supporting tables) alongside the app's own entities from Week 3.
public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<Tag> Tags => Set<Tag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Must call base first — IdentityDbContext's OnModelCreating
        // configures all the Identity tables, and skipping this call
        // would break Identity's schema entirely.
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TaskItem>()
            .HasMany(t => t.Tags)
            .WithMany(tag => tag.Tasks)
            .UsingEntity(j => j.ToTable("TaskTags"));

        

        modelBuilder.Entity<Project>()
            .Property(p => p.Budget)
            .HasPrecision(18, 2);
    }
}