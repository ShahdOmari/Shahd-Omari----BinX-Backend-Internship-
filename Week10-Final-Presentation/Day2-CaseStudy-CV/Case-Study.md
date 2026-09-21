# Technical Case Study — Cardiac Patient Monitoring System

**Developer:** Shahd Omari
**Program:** BinX Tech .NET Backend Internship · 2026
**Duration:** 4 sprints · Weeks 6–9 · ~160 hours
**Stack:** ASP.NET Core 10 · EF Core · SQL Server · Redis · JWT · GitHub Actions · Railway

---

## The Problem

A cardiac monitoring system is not a generic CRUD application.
Three constraints shaped every architectural decision:

1. **Who sees what:** A Nurse recording vital signs must not see another
   nurse's readings, or access the list of all critical patients across
   the ward — that is a Doctor's view. Role-based access is a clinical
   safety requirement, not just a feature.

2. **Performance at scale:** A naive implementation that loads all vital
   sign readings into memory and filters in C# works fine with 2 test
   patients. With 300 readings per patient across a full ward, it becomes
   a full table load on every request to the most time-sensitive endpoint
   in the system — the one that shows which patients are in critical condition.

3. **Traceability:** In healthcare, every data access must be logged.
   Who viewed a patient record, when, and from which role — not as an
   optional audit trail, but as a compliance requirement.

---

## Architecture
Request
↓
Rate Limiter (5 login/min, 100 general/min)
↓
JWT Authentication (ASP.NET Core Identity + StackExchange JWT)
↓
Role Authorization ([Authorize(Roles = "Doctor,Admin")])
↓
Ownership Check (staffProfileId claim vs RecordedByStaffProfileId)
↓
Controller Action
↓
AuditLoggingMiddleware (logs after _next — claims and status code resolved)

**Why this layering order matters:**
AuditLoggingMiddleware is placed *after* UseAuthentication and *after*
`await _next(context)`. If placed before, every request would log as
anonymous even with a valid JWT, because the authentication middleware
has not yet parsed the token. This is one of those decisions that is
invisible when correct and catastrophic when wrong.

**Role model:**
Generic Admin/User roles were rejected in favour of domain-appropriate ones:
Nurse, Doctor, Auditor, Admin — each maps to a real clinical responsibility.
"A Nurse cannot prescribe medication" is immediately defensible; "a User
cannot access Admin endpoints" is not.

**staffProfileId in the JWT:**
Embedding the domain entity ID in the token at login time eliminates
one database round-trip per request for ownership checks. Every `GET
/VitalSigns/{id}` call compares the claim directly against the stored
`RecordedByStaffProfileId` — no extra query needed.

---

## Biggest Engineering Challenge — The Full Table Load

### Discovery (Sprint 3, Day 1)

After enabling EF Core query logging:

```sql
-- GetCriticalPatients was generating:
SELECT [v].* FROM [VitalSigns] AS [v]
-- No WHERE. No TOP. 300 rows transferred to RAM.
```

The C# code then called `.GroupBy().Where().First()` in memory.
With 2 test patients the response time was under 40ms — fast enough
to look correct. With 300 readings the problem became measurable.
With 300,000 readings in a real hospital system, this endpoint would
be a production outage on every call.

### First fix attempt — and why it failed

```csharp
// This compiles. EF Core 10 cannot translate it to SQL.
_context.VitalSigns
    .GroupBy(v => v.PatientId)
    .Select(g => g.OrderByDescending(v => v.RecordedAtUtc).First())
    .Where(v => v.RiskLevel == RiskLevel.Critical)
    .ToListAsync();
// → KeyNotFoundException at runtime
```

EF Core 10 throws a `KeyNotFoundException` when it encounters this
pattern — it can compile the LINQ expression tree but cannot translate
`GroupBy(...).Select(g => g.First())` to SQL. This is a documented
ORM boundary that only surfaces at runtime, not at compile time.

### The working solution — correlated subquery

```sql
-- What the fixed version generates:
SELECT [v].*
FROM [VitalSigns] AS [v]
WHERE [v].[RiskLevel] = N'Critical'
  AND [v].[RecordedAtUtc] = (
      SELECT MAX([v0].[RecordedAtUtc])
      FROM [VitalSigns] AS [v0]
      WHERE [v0].[PatientId] = [v].[PatientId]
        AND [v0].[RiskLevel] = N'Critical')
```

**Result:** 300 rows → 7. The filtering, grouping, and MAX aggregation
all execute inside SQL Server. The network carries only the result.

### Lesson

The only reliable way to confirm EF Core is doing what the LINQ
expresses is to read the generated SQL. Enabling `.LogTo(Console.WriteLine)`
in development made the invisible visible — and revealed the problem
that would have been invisible in production until traffic scaled.

---

## Performance Results

All measurements taken from terminal logs, not benchmark tooling.

### Query optimization

| Endpoint | Before | After |
|---|---|---|
| GET /VitalSigns/critical | `SELECT *` — 300 rows, no WHERE | Correlated subquery — 7 rows |
| GET /VitalSigns | No patient name — client forced extra calls | INNER JOIN — PatientName included |

### Redis caching (GET /Patients)
Cache MISS (first request): 2651ms
Cache HIT (second request): 52ms
Cache HIT (third request): 9ms
Cache HIT (fourth request): 3ms

**98% response time reduction** on cache hits.

Cache invalidation confirmed: `POST /Patients` triggers `CACHE DEL`
immediately — the next `GET /Patients` reflects the new patient without
waiting for expiry.

### Database indexes

Three composite indexes added via EF Core Fluent API:

```csharp
builder.Entity<VitalSign>()
    .HasIndex(v => new { v.RiskLevel, v.RecordedAtUtc })
    .HasDatabaseName("IX_VitalSigns_RiskLevel_RecordedAtUtc")
    .IsDescending(false, true);
```

**Why composite, not two separate indexes:**
The critical query filters on `RiskLevel` AND orders by `RecordedAtUtc`.
Two separate indexes force SQL Server to use one for the filter and sort
the results in memory. One composite index covers both in a single seek.

After indexes, warm query time on `GET /VitalSigns/critical`: **50ms**.

---

## Test Suite

**34 tests · 34 passing**

Coverage spans unit tests (CardiacRiskEvaluator logic, VitalSignService),
integration tests (real HTTP requests via WebApplicationFactory with SQLite
in-memory), and targeted gap-closing tests added in Sprint 4:

- Auth error paths: wrong password → 401, duplicate email → 400
- RBAC: Nurse → 403 on Doctor-only endpoints
- Ownership: Nurse B → 403 on a reading recorded by Nurse A
- Core workflow: POST /VitalSigns → 201 with correct body

**Key design decision:** the test factory replaces Redis with
`AddDistributedMemoryCache()` — same interface, no external service.
This is why all 34 tests run in CI with zero infrastructure setup.

---

## CI/CD Pipeline
Push to main
↓
[build-and-test] ubuntu-latest · .NET 10 · SQLite in-memory
↓ only if passing
[deploy] Railway · production environment

A deliberate test break was pushed to confirm the pipeline fails visibly
(❌ in GitHub Actions), then fixed to confirm it returns to green (✅).
Broken code cannot silently reach production.

---

## Outcome

| Item | Status |
|---|---|
| Live REST API | Railway deployment — `GET /health` → 200 |
| Test suite | 34/34 passing |
| CI/CD pipeline | Green — build, test, deploy on every push to main |
| Performance | Cache hit 98% faster · 97% fewer rows on critical endpoint |
| Documentation | Swagger UI self-documenting · full setup README |
| Security | 4-layer model: rate limit → auth → RBAC → ownership |

---

## What I Would Build Differently

**1. Coverage audits at the end of every sprint, not only Sprint 4**
The ownership check and RBAC tests added in Sprint 4 covered gaps that
existed since Sprint 2. A 15-minute audit at the end of each sprint
would have caught them incrementally rather than accumulating them.

**2. Query-count regression test immediately after the fix**
The correlated subquery fix was confirmed manually by reading the terminal.
A test asserting `GetCriticalPatients` executes exactly 1 SQL statement
would make that fix permanent — the next developer who accidentally
reverts it would see a failing test, not a slow endpoint in production.

**3. The README before the code**
The project README was written at the end of Sprint 4. Writing it at the
start of each sprint — even as a one-paragraph description of the sprint
goal — would have kept the documentation in sync with the work throughout.
