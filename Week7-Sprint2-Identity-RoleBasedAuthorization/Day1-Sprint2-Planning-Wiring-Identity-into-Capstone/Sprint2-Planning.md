# Day 1 — Sprint 2 Planning & Identity Model Extension

### Week 7 · Phase 3, Sprint 2 · Cardiac Patient Monitoring System

---

## Sprint 2 Goal

> Extend the existing Identity/RBAC foundation (built in Weeks 1-4) with a
> domain-specific user profile and a third, read-only compliance role — carrying
> forward Sprint 1's retrospective action of sizing constructor/contract changes
> and their test updates as explicit, separate backlog items.

## Sprint 1 Retrospective Action — Applied Here

Sprint 1 ended with: *"When sizing any backlog task that changes a class's
constructor or public contract, explicitly include 'update existing tests
referencing this class' as its own line item."* Applied directly to today's
backlog below — every task that touches `AppDbContext`, `AuthController`, or
`Program.cs` has a matching "update dependent tests" line.

## Why This Sprint's Work Differs From the Lesson's Starting Point

Unlike a fresh capstone wiring up Identity for the first time, this project
already has `IdentityDbContext<IdentityUser>`, JWT issuance, and two working roles
(Nurse/Doctor) from Week 4. Sprint 2's genuine, non-redundant work here is:
**extending the user model itself** with real domain fields, and **planning a
third role** most student projects never add — both are realistic next steps for
an already-authenticated system, not a first-time integration.

## Sprint 2 Backlog (sized ~half-day to a day each)

| Task | Size | Notes |
|---|---|---|
| Design `ApplicationUser : IdentityUser` with `FullName` and `LicenseNumber` | 0.5 day | New — extends the default Identity user |
| Update `AppDbContext` to `IdentityDbContext<ApplicationUser>` | 0.5 day | Contract change — update `Program.cs` registrations |
| Update `AuthController`/`Program.cs` references from `IdentityUser` to `ApplicationUser` | 0.5 day | Contract change — **update any test referencing these types** (Sprint 1 retro action) |
| Generate + review + apply migration for the new user columns | 0.5 day | Must be additive only, non-destructive to existing data |
| Plan and document the third role: `Auditor` | 0.5 day | Read-only compliance role — no write access anywhere |
| Document role-to-endpoint matrix | 0.5 day | Every endpoint mapped to which role(s) can call it |

## Planned Role Structure

| Role | Purpose | Can Do |
|---|---|---|
| **Nurse** | Day-to-day patient care | Record vitals, view patients/medications/appointments |
| **Doctor** | Clinical decision-making | Everything Nurse can, plus create/update/delete medications, delete patients |
| **Auditor** *(new)* | Compliance/regulatory review | **Read-only** access to all resources — cannot create, update, or delete anything, anywhere |

## Planned Role-to-Endpoint Matrix

| Endpoint | Nurse | Doctor | Auditor |
|---|---|---|---|
| `GET /Patients`, `GET /VitalSigns`, `GET /Medications`, `GET /Appointments` | ✅ | ✅ | ✅ |
| `POST /Patients`, `POST /VitalSigns`, `POST /Appointments` | ✅ | ✅ | ❌ |
| `POST /Medications`, `DELETE /Medications` | ❌ | ✅ | ❌ |
| `DELETE /Patients` | ❌ | ✅ | ❌ |

This matrix is the plan enforced with `[Authorize(Roles = ...)]` starting Day 2 —
deciding it here on Day 1, before writing a single new attribute, is exactly what
the lesson's Section 1.3 recommends.

## Update — Migration Conflict Encountered & Resolved

While applying the migration extending `IdentityUser` to `ApplicationUser`, a
`Cannot insert duplicate key row` error occurred on `AspNetRoles`. Root cause: the
Nurse/Doctor roles seeded via `HasData` in Week 6 collided with roles already
created at runtime earlier (via the `/assign-role` endpoint's
`RoleManager.CreateAsync` fallback, used during manual Swagger testing before the
migration-level seed existed). Resolved by dropping and recreating the local
development database so all migrations — including the role seed — apply cleanly
from a known state, rather than patching around a database that had drifted from
what the migration history assumed.

**Lesson carried forward:** relying on both a runtime "create if missing" fallback
*and* a migration-level `HasData` seed for the same reference data is a source of
drift between environments — one should be the single source of truth going
forward.
