# Week 8 — Sprint 3: Advanced Queries & Performance

## Sprint Goal
Make the Cardiac Monitoring API measurably faster — every claim backed
by real before/after evidence from the terminal, not estimates.

## Days

| Day | Focus | Key Deliverable |
|-----|-------|-----------------|
| 1 | Sprint planning + N+1 diagnosis | EF Core logging enabled, 300-reading seed, full table load confirmed |
| 2 | Query optimization | Correlated subquery: 300 rows → 7, PatientName JOIN projection |
| 3 | Redis caching | Cache-aside on GET /Patients: miss=2651ms, hit=3ms |
| 4 | Database indexes + PR | 3 composite indexes, full table scan → index seek |
| 5 | Sprint close-out | Demo, DoD audit, retrospective, sprint summary |

## Performance Results

| Change | Before | After |
|---|---|---|
| GetCriticalPatients rows | 300 | 7 (-97%) |
| GET /Patients (cache hit) | ~45ms | 3ms (-98%) |
| VitalSigns critical warm | — | 50ms |

## Test Suite: 26/26 ✅ throughout all 5 days

## Code
All runnable code in `../Cardiac-Monitoring-System/`. Day folders contain
snapshots + README documenting what was built, problems hit, and how they
were resolved.
