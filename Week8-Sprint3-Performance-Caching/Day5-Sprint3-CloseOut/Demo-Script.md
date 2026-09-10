# Sprint 3 Demo Script — Performance Evidence

> Estimated time: 10-12 minutes
> Required: Postman open, `dotnet run` running, terminal visible

---

## Setup
```powershell
cd Cardiac-Monitoring-System\src\CardiacMonitoring.Api
dotnet run
```
Open Postman → Cardiac Monitoring System collection → Cardiac-Local environment.

---

## Scene 1 — The Problem We Diagnosed (2 min)

**Narrate:** "Day 1, I enabled EF Core query logging — one line in Program.cs that
prints every SQL statement to the terminal. Then I seeded 300 vital sign readings
instead of the original 2, because performance problems hide behind small datasets."

**Action:** Show the original GetCriticalPatients code (slide or comment in code):
```csharp
var allVitals = await _repository.GetAllAsync(); // loads ALL 300 rows
var latestCriticalPerPatient = allVitals
    .GroupBy(v => v.PatientId)         // C# memory
    .Select(g => g.OrderByDescending(v => v.RecordedAtUtc).First())
    .Where(v => v.RiskLevel == RiskLevel.Critical)
    .ToList();
```

**Point out:** "The SQL this generates is `SELECT * FROM VitalSigns` — no WHERE,
no TOP. 300 rows cross the network to RAM so C# can filter them down to 7.
At 300,000 patients this endpoint would bring the server down."

---

## Scene 2 — Fix 1: Correlated Subquery (2 min)

**Action:** GET `/api/v1/VitalSigns/critical` with Admin token

**Show terminal:**
```sql
WHERE [RiskLevel] = N'Critical'
  AND [RecordedAtUtc] = (
      SELECT MAX([v0].[RecordedAtUtc])
      FROM [VitalSigns] AS [v0]
      WHERE [v0].[PatientId] = [v].[PatientId]
        AND [v0].[RiskLevel] = N'Critical')
```

**Key point:** "SQL Server now filters Critical rows and finds the latest per patient
entirely inside the database. 7 rows cross the network instead of 300 — 97% reduction.
I tried GroupBy+First first but EF Core 10 throws a KeyNotFoundException at runtime —
it can compile the LINQ but cannot translate it to SQL. The correlated subquery is
the correct pattern."

---

## Scene 3 — Fix 2: Projection with Patient Name (1 min)

**Action:** GET `/api/v1/VitalSigns` with Admin token

**Show terminal:**
```sql
SELECT [v].[Id], [v].[PatientId], [p].[FullName], ...
FROM [VitalSigns] AS [v]
INNER JOIN [Patients] AS [p] ON [v].[PatientId] = [p].[Id]
```

**Key point:** "Before, the response had no patient name. The frontend would need
one GET /Patients/{id} per row — client-side N+1. One JOIN projection eliminates it."

---

## Scene 4 — Fix 3: Redis Cache Hit/Miss (3 min)

**Action 1:** GET `/api/v1/Patients` — first time

**Show terminal:**
CACHE MISS | key=patients:page=1&size=10&gender=&minAge=&sort=name&dir=asc
Executed DbCommand (8ms) SELECT ... FROM [Patients]
AUDIT | GET /api/v1/Patients | 2651ms

**Action 2:** GET `/api/v1/Patients` — second time

**Show terminal:**
CACHE HIT | key=patients:page=1&size=10&gender=&minAge=&sort=name&dir=asc
AUDIT | GET /api/v1/Patients | 3ms

**Key point:** "2651ms to 3ms — 98% faster. The first call is slow because it also
writes to Redis. Every subsequent call skips the database entirely."

**Action 3:** POST `/api/v1/Patients` → then GET `/api/v1/Patients`

**Show terminal:**
CACHE DEL | key=patients:...
CACHE MISS | key=patients:...

**Key point:** "Invalidation is explicit and immediate. A cache with no invalidation
is not a cache — it is a slowly compounding data-correctness bug."

---

## Scene 5 — Fix 4: Database Indexes (2 min)

**Narrate:** "I added three indexes via EF Core Fluent API and a migration — additive
only, zero downtime, safe on a live database."

**Show the indexes:**
IX_VitalSigns_RiskLevel_RecordedAtUtc → critical endpoint
IX_VitalSigns_PatientId_RecordedAtUtc → list + JOIN queries
IX_Medications_PatientId → medication filter by patient

**Action:** GET `/api/v1/VitalSigns/critical` three times

**Show timing:**
Request 1 (cold): 387ms
Request 2 (warm): 92ms
Request 3 (warm): 50ms

**Key point:** "Why composite and not two separate indexes? The critical query filters
on RiskLevel AND sorts by RecordedAtUtc. Two separate indexes force SQL Server to
use one for the filter and sort the results in memory. One composite index covers
both in a single seek."

---

## Closing (30 sec)
"Every number I showed came from the terminal — not an estimate. The pattern across
all four fixes is the same: push work to the layer designed for it. Filtering belongs
in SQL, not C#. Repeated reads belong in a cache. Lookups belong on an index."
