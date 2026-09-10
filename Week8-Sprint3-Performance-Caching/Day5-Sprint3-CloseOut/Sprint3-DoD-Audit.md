# Sprint 3 — Definition of Done Audit

## Acceptance Criteria Check

| # | Backlog Item | Target | Status | Evidence |
|---|---|---|---|---|
| 1 | Enable EF Core query logging | SQL visible in terminal | ✅ Done | `LogTo(Console.WriteLine)` in Program.cs, development only |
| 2 | Seed realistic data volume | 50+ records in primary tables | ✅ Done | 20 patients, 300 VitalSigns, 40 Medications, 40 Appointments |
| 3 | Diagnose N+1 / full table load | Identify at least 1 genuine problem | ✅ Done | GetCriticalPatients: SELECT * no WHERE, 300 rows to RAM |
| 4 | Fix GetCriticalPatients | ≤2 SQL queries, push filter to DB | ✅ Done | Correlated subquery: 1 query, 7 rows (was 300) |
| 5 | Fix GetAll VitalSigns | Client N+1 eliminated | ✅ Done | INNER JOIN projection includes PatientName |
| 6 | Redis cache on GET /Patients | Cache hit measurably faster | ✅ Done | Miss=2651ms → Hit=3ms (98% improvement) |
| 7 | Cache invalidation on writes | New/updated data appears immediately | ✅ Done | CACHE DEL confirmed on POST/PUT/DELETE |
| 8 | At least 1 composite index | Composite index added and applied | ✅ Done | 2 composite indexes on VitalSigns |
| 9 | Before/after measurements | Real numbers, not estimates | ✅ Done | All numbers from terminal logs |
| 10 | Pull Request with evidence | PR opened on GitHub | ✅ Done | week8/sprint3-performance → main |
| 11 | Test suite green throughout | 26/26 passing after each change | ✅ Done | Verified after Day 2, 3, and 4 |

## All 11 items: DONE ✅

---

## Sprint 4 Backlog — Remaining Opportunities

| Tag | Item | Why Deferred |
|---|---|---|
| `[cache]` | Redis SCAN to invalidate all `patients:*` key variants (filtered pages) — current invalidation only removes the default key | Requires `IConnectionMultiplexer` directly; out of scope for Sprint 3 |
| `[cache]` | Cache hit/miss ratio metric exposed via a `/health` or `/diagnostics` endpoint | Nice-to-have; not a correctness issue |
| `[index]` | Index on `Appointments.PatientId` — same pattern as Medications | Not a hot query in current usage; add when query logging shows it is |
| `[index]` | Index on `Appointments.ScheduledAtUtc` — for date-range queries | Deferred pending confirmation that date-range queries are common |
| `[perf]` | Pagination on `GET /VitalSigns` — currently returns all 300 readings | Requires a breaking change to the response shape; Sprint 4 candidate |
| `[perf]` | Response compression (gzip) on large list endpoints | Infrastructure concern; belongs with deployment work in Sprint 4 |
| `[test]` | Automated test asserting GetCriticalPatients query count stays at 1 | Turns this sprint manual check into a permanent regression guard |
