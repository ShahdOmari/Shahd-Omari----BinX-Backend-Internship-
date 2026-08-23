# Day 1 — Sprint 1 Planning & Project Database Design

### Week 6 · BinX Backend Development Internship (.NET) — Phase 3, Sprint 1
### Capstone Project: Cardiac Patient Monitoring System

---

## Sprint Goal

> A complete, normalized database schema for the Cardiac Monitoring capstone —
> already implemented as EF Core migrations across Weeks 1-4 — is formally
> documented as an ERD and confirmed against the full professional baseline before
> Sprint 2 (authentication/RBAC hardening) builds on top of it.

## Note on Timing

Unlike a project starting fresh this sprint, this capstone's schema and core routes
were already built and tested across Weeks 1-5 (see `Cardiac-Monitoring-System/README.md`
for full details). Sprint 1's work here is the formal planning artifact this lesson
requires: a written sprint goal, the finalized ERD, and a sized backlog — applied
retroactively to confirm the existing schema meets the full professional baseline,
not just re-doing work already complete.

## Full Entity List (Professional Baseline)

Every entity the finished capstone needs, not just what was originally scoped:

| Entity | Purpose | Status |
|---|---|---|
| `Patient` | Core patient profile | ✅ Implemented |
| `VitalSign` | Vital-sign readings + computed risk level | ✅ Implemented |
| `Medication` | Prescribed medications per patient | ✅ Implemented |
| `Appointment` | Scheduled appointments per patient | ✅ Implemented |
| `AspNetUsers` / `AspNetRoles` (Identity) | Nurse/Doctor accounts and roles | ✅ Implemented |

## Sprint 1 Backlog (sized ~half-day to a day each)

| Task | Size | Status |
|---|---|---|
| Design `Patient` entity + migration | 0.5 day | ✅ Done (Week 1-4) |
| Design `VitalSign` entity + migration + FK to Patient | 0.5 day | ✅ Done |
| Design `Medication` entity + migration + FK to Patient | 0.5 day | ✅ Done |
| Design `Appointment` entity + migration + FK to Patient | 0.5 day | ✅ Done |
| Implement `GET /Patients` and `GET /Patients/{id}` | 0.5 day | ✅ Done |
| Implement `POST /Patients` core creation flow | 0.5 day | ✅ Done |
| Finalize ERD and confirm 3NF normalization | 0.5 day | ✅ Done today |

## Definition of Done for Sprint 1

- [x] Schema covers every entity needed for the full professional baseline
- [x] All relationships enforced with foreign keys and cascade delete
- [x] Migrations applied and reproducible via `dotnet ef database update`
- [x] ERD documented and committed to the repository (see `ERD.md`)
- [x] Core routes (Patients CRUD) working end-to-end
