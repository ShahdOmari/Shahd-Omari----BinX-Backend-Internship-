# Sprint 4 Final Presentation Outline
## Cardiac Patient Monitoring System — BinX Tech Internship Week 10

---

## Slide 1 — Title
**Cardiac Patient Monitoring System**
Shahd Omari · BinX Tech .NET Backend Internship · 2026
Live API · GitHub · CI/CD Pipeline

---

## Slide 2 — The Problem
A cardiac monitoring system needs more than just CRUD endpoints.
It needs:
- **Who** can see patient data (authentication)
- **What** each role is allowed to do (authorization)
- **Whose** readings belong to whom (ownership)
- **Fast** responses under real patient volumes (performance)
- **Reliable** — broken code must never reach production (CI/CD)

---

## Slide 3 — Architecture Overview
Client
↓
[Rate Limiter] 5 req/min login, 100 req/min general
↓
[JWT Authentication] who are you?
↓
[Role Authorization] what are you allowed to do?
↓
[Ownership Check] is this specific resource yours?
↓
[Controller Action]
↓
[AuditLoggingMiddleware] every request logged

4 controllers · 5 roles · Redis cache · 3 composite indexes

---

## Slide 4 — Key Technical Decisions

**1. Domain-appropriate roles (not generic Admin/User)**
Nurse · Doctor · Auditor · Admin — each maps to a real clinical responsibility

**2. staffProfileId in the JWT**
Embedding the domain entity ID at login time eliminates one DB query
per request for ownership checks — zero extra round-trips

**3. ICacheService abstraction over IDistributedCache**
If Redis goes down, every method degrades gracefully to the DB
instead of returning 500

**4. Correlated subquery over GetAllAsync()**
GetCriticalPatients fetches 7 rows instead of 300 — filtering in SQL,
not in C# memory

---

## Slide 5 — Biggest Challenge: The Full Table Load

**The problem discovered (Sprint 3, Day 1):**
```sql
-- What the code was doing:
SELECT * FROM VitalSigns   -- 300 rows, no WHERE

-- C# then filtered in memory:
.GroupBy(...).Where(...).First()
```

**Why it was invisible in development:** 2 patients, 2 readings — looks fine.

**The fix:**
```sql
-- What it does now:
WHERE [RiskLevel] = N'Critical'
  AND [RecordedAtUtc] = (SELECT MAX(...) WHERE PatientId = v.PatientId)
-- Returns 7 rows
```

**Lesson:** The only reliable way to diagnose this is to watch the actual SQL.
Query logging made the invisible visible.

---

## Slide 6 — Performance Results (measured, not estimated)

| Change | Before | After |
|---|---|---|
| GetCriticalPatients rows | 300 | 7 (-97%) |
| GET /Patients cache miss | ~45ms | 30ms |
| GET /Patients cache hit | ~45ms | 3ms (-98%) |
| Critical endpoint (warm) | table scan | 50ms index seek |

Every number from the terminal — not a benchmark tool, not an estimate.

---

## Slide 7 — Test Suite

**34 tests · 34 passing**

| Suite | Tests |
|---|---|
| Unit — CardiacRiskEvaluator | 10 |
| Unit — VitalSignService | 4 |
| Integration — Patients | 3 |
| Integration — VitalSigns | 6 |
| Integration — RoleBasedAccess | 5 |
| Integration — BusinessRules | 2 |
| Integration — CoverageGaps | 8 |

Key coverage: Auth error paths · RBAC on every clinical endpoint ·
Ownership check (Nurse A ≠ Nurse B's readings)

---

## Slide 8 — CI/CD Pipeline
Push to main
↓
[build-and-test] ubuntu-latest · .NET 10 · 34/34 tests
↓ only if passing
[deploy] Railway · production environment

Demo: deliberate test break → pipeline ❌ → fix → pipeline ✅

**Why it matters:** broken code cannot silently reach production.

---

## Slide 9 — Live Demo
- `GET /health` → 200 (Railway)
- Login as Admin → JWT with staffProfileId claim
- `POST /VitalSigns` as Nurse → 201
- `GET /VitalSigns/critical` as Nurse → 403
- `GET /VitalSigns/critical` as Doctor → 200, 7 results
- `GET /Patients` twice → CACHE HIT · 3ms

---

## Slide 10 — What I Would Do Differently
1. Run a coverage audit at the end of each sprint, not only Sprint 4
2. Write the README before the code, not after
3. Add automated query-count regression tests immediately after the fix
   (not just in the Sprint 4 backlog)

---

## Slide 11 — Outcome
✅ Live REST API on Railway
✅ 34/34 tests — unit + integration
✅ CI/CD pipeline — build, test, deploy on every push
✅ Redis caching — 98% response time improvement
✅ Composite database indexes
✅ Role-based access across all 5 controllers
✅ Swagger documentation — self-documenting API
