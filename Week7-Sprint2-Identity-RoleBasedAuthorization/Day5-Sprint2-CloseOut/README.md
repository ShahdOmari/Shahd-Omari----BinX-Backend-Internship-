# Day 5 — Sprint 2 Close-Out

## Sprint Context
Sprint 2, Week 7, Day 5 — final day.

## What Was Done

### 1. Demo (Postman)
Full authentication and RBAC flow demoed covering 6 scenes:
- Registration creates User + StaffProfile atomically
- Login issues JWT with domain claims (`staffProfileId`, `role`)
- **Rejection Case 1:** Nurse token → `403` on `GET /VitalSigns` (role boundary)
- **Rejection Case 2:** Nurse B token → `403` on VitalSign recorded by Nurse A (ownership)
- Audit trail visible in terminal for every request including rejections
- Admin-only `assign-role` rejects Nurse token with `403`

See `Demo-Script.md` for the full narrated walkthrough.

### 2. Definition of Done Audit
All 13 Sprint 2 backlog items verified against acceptance criteria — all complete. See `Sprint2-DoD-Audit.md`.

### 3. Sprint 3 Backlog — Edge Cases
5 authorization edge cases discovered during Sprint 2 tagged and deferred:
- `[auth-edge]` Dual-role precedence undefined
- `[auth-edge]` `assign-role` accepts arbitrary role strings
- `[audit]` No persistent audit log sink
- `[auth-edge]` Token revocation not implemented
- `[ownership]` Patients have no assigned responsible staff member

### 4. Sprint Retrospective
See `Sprint2-Retrospective.md` for full what-went-well / what-to-improve / one-concrete-action writeup.

### 5. Sprint Summary
See `Sprint2-Summary.md` — includes registration/login flow diagram, full RBAC matrix, test suite breakdown, and PR link.

## Key Takeaway
The most valuable practice this sprint was running `dotnet test` after every contract change — it caught three separate regressions (missing `department` field, stale RBAC assertion, rate limiter throttling test logins) that would have been invisible until runtime in a project without tests.
