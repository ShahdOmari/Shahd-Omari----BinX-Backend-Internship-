# Week 7 — Sprint 2: Identity & Role-Based Authorization

Second sprint of the Cardiac Patient Monitoring System capstone.

## Sprint Goal
Wire ASP.NET Core Identity into the capstone, issue JWTs with domain-specific claims, enforce real role-based access control across every endpoint, and add an audit trail middleware.

## Days
| Day | Focus | Key Deliverable |
|-----|-------|-----------------|
| 1 | Sprint planning + Identity wiring | `ApplicationUser`, `StaffProfile` entity, role model (Nurse/Doctor/Auditor) |
| 2 | JWT registration & login | `StaffProfile` linked at registration, `staffProfileId` JWT claim |
| 3 | RBAC + Ownership checks | Role requirements on every endpoint, ownership check on VitalSigns |
| 4 | Custom middleware + PR | `AuditLoggingMiddleware`, Sprint 2 Pull Request |

## Roles Used (domain-appropriate, not generic Admin/User)
- **Nurse** — default at registration; records vitals, views patients
- **Doctor** — explicitly granted; full clinical access including prescribing and deleting
- **Auditor** — explicitly granted; read-only access for compliance review
- **Admin** — seeded once at startup; manages roles and system-level operations

## Test Suite
26 tests, 26 passing at end of Sprint 2.

## Code
All runnable code lives in `../Cardiac-Monitoring-System/`. Each day folder here contains read-only snapshots + README.
