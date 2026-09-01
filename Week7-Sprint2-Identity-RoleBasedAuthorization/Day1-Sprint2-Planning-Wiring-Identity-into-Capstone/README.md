# Day 1 — Sprint 2 Planning & Wiring Identity into the Capstone

### Week 7 · Phase 3, Sprint 2

---

## Learning Objectives

- Define the Sprint 2 goal, carrying forward the Sprint 1 retrospective's improvement.
- Integrate ASP.NET Core Identity into the capstone project's existing DbContext.
- Plan which roles the project's specific domain actually needs.

## What I Did

- Wrote the Sprint 2 goal and backlog, explicitly applying Sprint 1's retrospective
  action: every task that changes a class's constructor or public contract has a
  separate "update dependent tests" line item (see `Sprint2-Planning.md`).
- Extended the existing `IdentityUser` to a custom `ApplicationUser` with two
  domain-relevant fields (`FullName`, `LicenseNumber`) — since this capstone
  already had Identity wired up since Week 4, this was a realistic *extension*
  of an existing model, not a first-time integration.
- Planned a third role beyond Nurse/Doctor: **Auditor**, a read-only compliance
  role with no write access anywhere in the API — documented with a full
  role-to-endpoint matrix before writing a single new `[Authorize]` attribute.
- Generated and reviewed the migration extending `AspNetUsers` with the new columns.

## A Real Migration Conflict Encountered — and the Fix

Applying the migration failed with a duplicate-key error on `AspNetRoles`. Root
cause: the Nurse/Doctor roles seeded via `HasData` in Week 6 collided with roles
already created at runtime earlier, through the `/assign-role` endpoint's
"create role if missing" fallback used during manual Swagger testing. Resolved by
resetting the local development database so all migrations — including the
migration-level role seed — apply from a clean, known state, rather than patching
around a database that had drifted from what the migration history assumed.

**Lesson:** relying on both a runtime fallback *and* a migration-level seed for
the same reference data is a source of environment drift — one should be the
single source of truth.

## Files in This Folder

- `Sprint2-Planning.md` — sprint goal, backlog, role plan, and the migration
  conflict write-up

## Key Takeaway

Deciding roles and reviewing migrations *before* writing authorization code is
what turns a mid-sprint database conflict into a documented, quickly-resolved
lesson instead of a blocking surprise discovered days later.