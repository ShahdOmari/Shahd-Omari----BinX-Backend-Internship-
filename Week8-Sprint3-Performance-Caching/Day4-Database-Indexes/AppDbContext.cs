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

        builder.Entity<StaffProfile>()
            .HasIndex(s => s.ApplicationUserId)
            .IsUnique();

        // ---- Sprint 3, Day 4: Performance Indexes ----
        //
        // Index 1: Composite index on (PatientId, RecordedAtUtc DESC)
        // Targets: GET /VitalSigns — filters by PatientId, orders by RecordedAtUtc.
        // A single-column index on PatientId would still require SQL Server to
        // sort the matching rows by RecordedAtUtc in a second pass. The composite
        // index covers both the filter and the sort in one index seek.
        builder.Entity<VitalSign>()
            .HasIndex(v => new { v.PatientId, v.RecordedAtUtc })
            .HasDatabaseName("IX_VitalSigns_PatientId_RecordedAtUtc")
            .IsDescending(false, true); // PatientId ASC, RecordedAtUtc DESC

        // Index 2: Composite index on (RiskLevel, RecordedAtUtc DESC)
        // Targets: GET /VitalSigns/critical — filters WHERE RiskLevel = 'Critical'
        // then finds the MAX(RecordedAtUtc) per patient via correlated subquery.
        // Without this index, SQL Server does a full table scan on every call to
        // the critical endpoint. With it, SQL Server seeks directly to Critical
        // rows, ordered by time — the MAX() subquery becomes an index range scan.
        builder.Entity<VitalSign>()
            .HasIndex(v => new { v.RiskLevel, v.RecordedAtUtc })
            .HasDatabaseName("IX_VitalSigns_RiskLevel_RecordedAtUtc")
            .IsDescending(false, true); // RiskLevel ASC, RecordedAtUtc DESC

        // Index 3: PatientId on Medications
        // Targets: GET /Medications?patientId=X — the most common medications
        // query filters by patient. Without an index, SQL Server scans all
        // medication rows regardless of how many patients exist.
        builder.Entity<Medication>()
            .HasIndex(m => m.PatientId)
            .HasDatabaseName("IX_Medications_PatientId");

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
