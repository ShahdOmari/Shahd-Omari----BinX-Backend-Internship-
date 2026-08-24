# Day 2 — Building the EF Core Data Model & Migrations

### Week 6 · Phase 3, Sprint 1

---

## Learning Objectives

- Model the full capstone domain as EF Core entity classes.
- Configure relationships explicitly using the Fluent API.
- Seed initial reference data and apply the migration.

## What I Did

- Added explicit Fluent API configuration for two relationships
  (`Patient` → `VitalSign`, `Patient` → `Medication`) in `OnModelCreating`,
  including an explicit `OnDelete(DeleteBehavior.Cascade)` decision — rather than
  relying on EF Core's implicit convention-based inference.
- Seeded the two roles the application depends on (`Nurse`, `Doctor`) directly via
  `HasData`, using fixed GUIDs rather than `Guid.NewGuid()` — required, since
  `HasData` values are baked into the migration file at design time.
- Generated the migration with `dotnet ef migrations add`, and reviewed the
  generated file before applying it — confirmed it contained only additive
  `InsertData` calls for the roles, with no unexpected `DropColumn`/`DropTable`.
- Applied the migration with `dotnet ef database update` and rebuilt successfully.

## Files in This Folder

- `AppDbContext.cs` — read-only snapshot of the updated DbContext for review. The
  real, buildable file lives at
  `Cardiac-Monitoring-System/src/CardiacMonitoring.Api/Data/AppDbContext.cs`.

## Key Takeaway

Reviewing a generated migration file before applying it is a five-minute habit that
catches schema mistakes — an unintended cascade delete, a missing constraint —
before they reach a real database, rather than after.
