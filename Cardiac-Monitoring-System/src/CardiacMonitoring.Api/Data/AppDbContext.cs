using CardiacMonitoring.Api.Entities;
using CardiacMonitoring.Api.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CardiacMonitoring.Api.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<VitalSign> VitalSigns => Set<VitalSign>();
    public DbSet<Medication> Medications => Set<Medication>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<StaffProfile> StaffProfiles => Set<StaffProfile>();
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
        
        // One ApplicationUser has exactly one StaffProfile — enforced with a
        // unique index on the foreign key, not just a plain index, since this is
        // a one-to-one relationship, not one-to-many.
        builder.Entity<StaffProfile>()
        .HasIndex(s => s.ApplicationUserId)
        .IsUnique();
        
        // ConcurrencyStamp is set explicitly here as a fixed GUID string, not left
// to auto-generate — HasData values must be fully static and deterministic
// at design time. Leaving ConcurrencyStamp unset let EF Core regenerate a
// different value on every model build, which is exactly what triggered
// the "model changes each time it is built" error.
builder.Entity<IdentityRole>().HasData(
    new IdentityRole
    {
        Id = "a1111111-1111-1111-1111-111111111111",
        Name = "Nurse",
        NormalizedName = "NURSE",
        ConcurrencyStamp = "c1111111-1111-1111-1111-111111111111"
    },
    new IdentityRole
    {
        Id = "b2222222-2222-2222-2222-222222222222",
        Name = "Doctor",
        NormalizedName = "DOCTOR",
        ConcurrencyStamp = "c2222222-2222-2222-2222-222222222222"
    });
    }
}
