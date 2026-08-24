using CardiacMonitoring.Api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CardiacMonitoring.Api.Data;

public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<VitalSign> VitalSigns => Set<VitalSign>();
    public DbSet<Medication> Medications => Set<Medication>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<VitalSign>()
            .Property(v => v.RiskLevel)
            .HasConversion<string>();

        // ---- Explicit Fluent API relationship configuration (Sprint 1, Day 2) ----
        // EF Core can infer these by convention, but configuring them explicitly
        // documents the intended cardinality and delete behavior directly in code
        // instead of leaving it to an implicit guess.
        builder.Entity<Patient>()
            .HasMany(p => p.VitalSigns)
            .WithOne(v => v.Patient)
            .HasForeignKey(v => v.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Patient>()
            .HasMany(p => p.Medications)
            .WithOne(m => m.Patient)
            .HasForeignKey(m => m.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        // ---- Reference data seeding via HasData (Sprint 1, Day 2) ----
        // Fixed GUIDs, not Guid.NewGuid() — HasData values are baked into the
        // migration at design time, so a randomly generated ID would produce a
        // different value every time "migrations add" runs, which EF Core rejects.
        builder.Entity<IdentityRole>().HasData(
            new IdentityRole
            {
                Id = "a1111111-1111-1111-1111-111111111111",
                Name = "Nurse",
                NormalizedName = "NURSE"
            },
            new IdentityRole
            {
                Id = "b2222222-2222-2222-2222-222222222222",
                Name = "Doctor",
                NormalizedName = "DOCTOR"
            });
    }
}
