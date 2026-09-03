# Week 7 — Sprint 2: Identity & Role-Based Authorization

## Overview
Second sprint of the Cardiac Patient Monitoring System capstone. Builds a complete, layered security model on top of Sprint 1's core routes — real ASP.NET Core Identity, JWT authentication with domain-specific claims, role-based access control across every endpoint, resource-level ownership checks, and a healthcare-grade audit trail.

## Sprint Goal (Achieved ✅)
Every API endpoint is protected by an appropriate access policy. A Nurse can record vital signs but cannot prescribe medication. A Doctor can see all readings across all patients. An Auditor can read everything but change nothing. An Admin can manage roles. Every request is traceable to a specific authenticated staff member.

## Roles (Domain-Appropriate)

| Role | Who | Default? |
|---|---|---|
| **Nurse** | Bedside staff recording vitals and managing patients | ✅ Assigned at registration |
| **Doctor** | Clinicians with full read/write access including prescribing | ❌ Granted by Admin |
| **Auditor** | Compliance reviewers — read-only, no writes | ❌ Granted by Admin |
| **Admin** | System administrator — manages roles, seeded at startup | ❌ Seeded once |

## Days

| Day | Focus | Key Deliverable |
|---|---|---|
| 1 | Sprint planning + Identity wiring | `ApplicationUser` extends `IdentityUser`; `StaffProfile` entity designed; role model finalized |
| 2 | Registration & login for capstone | Atomic transaction creates User + StaffProfile; JWT includes `staffProfileId` domain claim |
| 3 | RBAC + Ownership checks | Role requirements on all 5 controllers; Nurse-level ownership enforced on VitalSigns |
| 4 | Custom middleware + Pull Request | `AuditLoggingMiddleware` logs every request; Sprint 2 PR opened |
| 5 | Sprint close-out | Demo script, DoD audit, retrospective, sprint summary, 5 edge cases tagged for Sprint 3 |

## Security Model (Layered)
Request
↓
[Rate Limiter] — blocks brute force (5 login attempts/min)
↓
[Authentication] — who are you? validates JWT signature & expiry
↓
[Authorization] — what are you allowed to do? checks Role claims
↓
[Ownership Check] — is this specific resource yours? checks staffProfileId claim
↓
[AuditLoggingMiddleware]— logs user, staff, role, status, response time
↓
Controller Action

## Test Suite: 26/26 ✅

| Suite | Tests |
|---|---|
| Unit — CardiacRiskEvaluator | 10 |
| Unit — VitalSignService | 4 |
| Integration — Patients | 3 |
| Integration — VitalSigns | 4 |
| Integration — RoleBasedAccess | 3 |
| Integration — BusinessRuleValidation | 2 |
| **Total** | **26** |

## Code
All runnable code lives in `../Cardiac-Monitoring-System/`. Each day folder contains read-only snapshots + a README documenting what was built, real bugs found, and how they were fixed.

## Sprint 3 Preview
5 authorization edge cases tagged and moved to Sprint 3 backlog (see `Day5-Sprint2-CloseOut/Sprint2-DoD-Audit.md`).
