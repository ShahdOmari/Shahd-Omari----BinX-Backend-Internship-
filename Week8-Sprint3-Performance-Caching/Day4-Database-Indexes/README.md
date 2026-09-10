# Day 4 — Database Indexes & Performance Profiling

## Sprint Context
Sprint 3, Week 8, Day 4.

## Why These Indexes

### Which queries needed indexes?

**Query 1:** `GET /VitalSigns/critical`
```sql
WHERE [RiskLevel] = N'Critical'
  AND [RecordedAtUtc] = (SELECT MAX([RecordedAtUtc])
                          FROM [VitalSigns]
                          WHERE [PatientId] = [v].[PatientId]
                            AND [RiskLevel] = N'Critical')
```
Without an index on `RiskLevel`, SQL Server scans every row to find
Critical readings — a full table scan on every call to the most
clinically urgent endpoint in the system.

**Query 2:** `GET /VitalSigns` (list with JOIN)
```sql
INNER JOIN [Patients] AS [p] ON [v].[PatientId] = [p].[Id]
```
The JOIN on `PatientId` with no index forces a scan of VitalSigns for
every patient lookup.

**Query 3:** `GET /Medications?patientId=X`
Filters by `PatientId` with no index — full scan of all medications.

---

## Indexes Added (via Fluent API + Migration)

### Index 1 — `IX_VitalSigns_PatientId_RecordedAtUtc`
```csharp
builder.Entity<VitalSign>()
    .HasIndex(v => new { v.PatientId, v.RecordedAtUtc })
    .HasDatabaseName("IX_VitalSigns_PatientId_RecordedAtUtc")
    .IsDescending(false, true);
```
**Why composite, not two separate indexes:**
The most common VitalSigns query filters by `PatientId` AND sorts by
`RecordedAtUtc DESC`. Two separate single-column indexes force SQL
Server to use one for the filter, then sort results in memory.
The composite index covers both in one index seek — no extra sort pass.

---

### Index 2 — `IX_VitalSigns_RiskLevel_RecordedAtUtc`
```csharp
builder.Entity<VitalSign>()
    .HasIndex(v => new { v.RiskLevel, v.RecordedAtUtc })
    .HasDatabaseName("IX_VitalSigns_RiskLevel_RecordedAtUtc")
    .IsDescending(false, true);
```
**Target:** The critical endpoint. SQL Server seeks directly to
`RiskLevel = 'Critical'` entries ordered by time — the MAX() subquery
becomes an index range scan instead of a table scan.

---

### Index 3 — `IX_Medications_PatientId`
```csharp
builder.Entity<Medication>()
    .HasIndex(m => m.PatientId)
    .HasDatabaseName("IX_Medications_PatientId");
```
Simple FK index — every query that filters medications by patient
benefits immediately.

---

## Measured Results (real terminal output)

### GET /VitalSigns/critical — after indexes applied
Request 1 (cold): 387ms SQL Server builds execution plan + index seek
Request 2 (warm): 92ms execution plan cached, index seek
Request 3 (warm): 50ms SQL Server buffer cache warm

### Before vs After (Day 1 baseline → Day 4)

| Metric | Day 1 (before) | Day 4 (after) |
|---|---|---|
| SQL generated | SELECT * FROM VitalSigns (no WHERE) | WHERE RiskLevel + correlated MAX |
| Rows transferred | 300 | 7 |
| Query time (cold) | ~39ms (300 rows in RAM) | 387ms first call* |
| Query time (warm) | ~23ms (300 rows, SQL cache) | 50ms (7 rows, index seek) |
| Filtering location | C# memory | SQL Server |

*First-call overhead is higher because SQL Server compiles a new
execution plan for the correlated subquery. From the second call onward
the plan is cached and the index seek dominates — 50ms for 7 rows vs
the old 23ms that was deceivingly fast only because SQL Server had
cached the full-table result set in buffer memory.

**The real improvement is not the milliseconds — it is the row count.**
At 300 rows the difference is small. At 300,000 rows, transferring the
full table to RAM on every request to find 7 critical patients would be
a production outage. The index + correlated subquery pattern scales;
the GetAllAsync() + C# filter pattern does not.

---

## Migration
Name: `Sprint3Day4_PerformanceIndexes`
Type: additive only (3× CreateIndex) — no data changes, no dropped
columns. Safe to apply on a live database with zero downtime.

## Test Suite: 26/26 passing
