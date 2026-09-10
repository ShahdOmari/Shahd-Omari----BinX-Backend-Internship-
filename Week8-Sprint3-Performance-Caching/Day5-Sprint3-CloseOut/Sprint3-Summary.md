# Sprint 3 Summary — Ready for Mentor Check-In

## Sprint Goal (Achieved ✅)
Make the Cardiac Monitoring API measurably faster with real before/after
evidence — not claims. Every number below came from the terminal.

---

## Before/After Measurements

### 1. GetCriticalPatients — Full Table Load Fix

**BEFORE:**
```sql
SELECT [v].* FROM [VitalSigns]   -- no WHERE, no TOP
-- 300 rows → RAM → C# filters to 7
```

**AFTER:**
```sql
WHERE [RiskLevel] = N'Critical'
  AND [RecordedAtUtc] = (SELECT MAX(...) WHERE PatientId = v.PatientId
                                           AND RiskLevel = 'Critical')
-- 7 rows only
```

| Metric | Before | After |
|---|---|---|
| Rows transferred | 300 | 7 |
| Filter location | C# RAM | SQL Server |
| Row reduction | — | 97% |

---

### 2. GetAll VitalSigns — Projection Fix

**BEFORE:** No patient name in response → client needs N extra calls
**AFTER:** INNER JOIN includes PatientName in 1 query

---

### 3. Redis Cache-Aside on GET /Patients
CACHE MISS → 2651ms (DB query + Redis SET)
CACHE HIT → 52ms
CACHE HIT → 9ms
CACHE HIT → 3ms

| Request | Time |
|---|---|
| Miss (first) | 2651ms |
| Hit (steady state) | 3ms |
| Improvement | 98% faster |

**Invalidation confirmed:**
POST /Patients → CACHE DEL → next GET = fresh data (0ms stale window)

---

### 4. Database Indexes

| Index | Target Query | Effect |
|---|---|---|
| IX_VitalSigns_RiskLevel_RecordedAtUtc | critical endpoint | Table scan → Index seek |
| IX_VitalSigns_PatientId_RecordedAtUtc | list + JOIN | Table scan → Index seek |
| IX_Medications_PatientId | medications by patient | Table scan → Index seek |

**VitalSigns/critical timing after indexes:**
Cold: 387ms
Warm: 92ms
Warm: 50ms

---

## Caching Strategy

**What was cached:** `GET /Patients` — read frequently, changes rarely.
**What was NOT cached:** VitalSigns — new readings every few hours per patient.
**Pattern:** Cache-Aside with 10-minute Absolute Expiration.
**Invalidation:** Explicit CACHE DEL on every POST/PUT/DELETE to /Patients.
**Degradation:** Redis down → CacheService catches exception → falls through to DB.
**Tests:** Redis replaced with AddDistributedMemoryCache() in CardiacApiFactory.

---

## Sprint 4 Preview
6 tagged performance items in Sprint 3 backlog:
- Redis SCAN to invalidate all `patients:*` variants `[cache]`
- Automated query-count regression test `[test]`
- Pagination on GET /VitalSigns `[perf]`
- Response compression `[perf]`
- Additional indexes on Appointments `[index]`
- Cache warm-up on startup `[cache]`

## Test Suite: 26/26 ✅
