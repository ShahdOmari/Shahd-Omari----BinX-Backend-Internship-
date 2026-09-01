# Day 2 — JWT Login & Registration for the Capstone Project

### Week 7 · Phase 3, Sprint 2

---

## Learning Objectives

- Link a domain entity to its corresponding Identity user.
- Implement a registration flow that creates both records together, consistently.
- Issue JWTs containing claims relevant to the project's own domain.

## What I Did

Since patients in this system are managed records rather than user accounts, the
domain-relevant entity to link to `ApplicationUser` is **`StaffProfile`** — the
working profile for a Nurse or Doctor (department, hire date) — rather than a
"Customer" as in the lesson's e-commerce example. Same underlying principle:
authentication concerns (`ApplicationUser`) stay cleanly separated from domain
profile data (`StaffProfile`), linked by a foreign key with a unique index
(one-to-one, not one-to-many).

- Added `StaffProfile` entity and a `1:1` Fluent API relationship to `ApplicationUser`.
- Rebuilt `AuthController.Register` to create the `ApplicationUser` and the linked
  `StaffProfile` in a single database transaction — exactly the same
  all-or-nothing pattern used for Sprint 1's Critical-reading follow-up scheduling.
  If the `StaffProfile` failed to save after the `ApplicationUser` was already
  created, the account would be able to log in but hit confusing errors on every
  domain-specific endpoint afterward — the transaction prevents that.
- Extended `GenerateJwtToken` to add a `staffProfileId` claim — lets every
  subsequent authenticated request resolve which staff profile a token belongs to
  directly from the token, without an extra database lookup by email per request.

## Verification (Postman)

1. Register with `{ email, password, department }` → `200` with `staffProfileId`
   in the response.
2. Confirmed both `AspNetUsers` and `StaffProfiles` rows exist and are correctly
   linked, directly in the database.
3. Login with the same credentials → `200` with a JWT.
4. Decoded the JWT at jwt.io → confirmed the `staffProfileId` claim is present
   and matches the database record.

## Files in This Folder

Read-only snapshots for review. Real, buildable files:
- `Cardiac-Monitoring-System/src/CardiacMonitoring.Api/Entities/StaffProfile.cs`
- `Cardiac-Monitoring-System/src/CardiacMonitoring.Api/Controllers/AuthController.cs`
- `Cardiac-Monitoring-System/src/CardiacMonitoring.Api/Data/AppDbContext.cs`

## Key Takeaway

A registration endpoint that creates the auth record but silently fails to create
the linked domain profile produces a user who can log in but breaks on every
subsequent domain-specific request — wrapping both creations in one transaction
is what prevents that half-finished state from ever existing.