using CardiacMonitoring.Api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CardiacMonitoring.Api.Data;

// Inheriting from IdentityDbContext<IdentityUser> brings in the full
// Identity schema (Users, Roles, UserRoles, and supporting tables)
// alongside our own domain entities, all in one database/migration.
public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<VitalSign> VitalSigns => Set<VitalSign>();
    public DbSet<Medication> Medications => Set<Medication>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Must call base first — IdentityDbContext's OnModelCreating sets
        // up all the Identity tables; skipping this would break Identity's
        // schema entirely.
        base.OnModelCreating(builder);

        builder.Entity<VitalSign>()
            .Property(v => v.RiskLevel)
            .HasConversion<string>();
    }
}
