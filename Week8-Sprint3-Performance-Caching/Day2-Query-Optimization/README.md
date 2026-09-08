# Day 2 — Query Optimization: Projection & Correlated Subquery

## Sprint Context
Sprint 3, Week 8, Day 2.

## Problems Fixed Today

### Fix 1: GetCriticalPatients — Full Table Load → Correlated Subquery

**BEFORE (Day 1 diagnosis):**
```sql
SELECT [v].* FROM [VitalSigns] AS [v]
-- 300 rows transferred to RAM, filtered in C#
```
- Query count: 1
- Rows transferred: 300
- Filtering: C# memory (GroupBy + Where + First)

**AFTER:**
```sql
SELECT [v].[Id], [v].[PatientId], [v].[HeartRateBpm], [v].[SystolicBp],
       [v].[DiastolicBp], [v].[OxygenSaturationPercent], [v].[RecordedAtUtc], [v].[RiskLevel]
FROM [VitalSigns] AS [v]
WHERE [v].[RiskLevel] = N'Critical'
  AND [v].[RecordedAtUtc] = (
      SELECT MAX([v0].[RecordedAtUtc])
      FROM [VitalSigns] AS [v0]
      WHERE [v0].[PatientId] = [v].[PatientId]
        AND [v0].[RiskLevel] = N'Critical')
```
- Query count: 1
- Rows transferred: 7 (only the latest Critical reading per patient)
- Filtering: SQL Server (WHERE + correlated MAX subquery)
- **Row reduction: 300 → 7 (~97% fewer rows)**

**Why this approach:** EF Core 10 cannot translate
`GroupBy(...).Select(g => g.OrderByDescending(...).First())` to SQL —
it throws a `KeyNotFoundException` at runtime. The correlated subquery
pattern (`WHERE RecordedAtUtc = MAX(...)`) produces identical results
and translates correctly. This is a real EF Core limitation worth
documenting: GroupBy-then-First is a common pattern that looks correct
in C# but fails silently or loudly depending on the provider version.

---

### Fix 2: GetAll VitalSigns — Projection with Patient Name

**BEFORE:**
```sql
SELECT [v].* FROM [VitalSigns] AS [v]
-- Response had no patient name; frontend would need GET /Patients/{id} per row → client-side N+1
```

**AFTER:**
```sql
SELECT [v].[Id], [v].[PatientId], [p].[FullName],
       [v].[HeartRateBpm], [v].[SystolicBp], [v].[DiastolicBp],
       [v].[OxygenSaturationPercent], [v].[RecordedAtUtc], [v].[RiskLevel]
FROM [VitalSigns] AS [v]
INNER JOIN [Patients] AS [p] ON [v].[PatientId] = [p].[Id]
```
- Query count: 1 (was 1, stays 1 — but now complete)
- New field: `patientName` in every response item
- No extra round-trip needed from the client

**Why projection over Include:** `Include(v => v.Patient)` would load
the full `Patient` entity — Id, FullName, DateOfBirth, Gender — even
though the caller only needs `FullName`. Projection with `Select` tells
SQL Server to fetch exactly the columns needed, nothing more.

---

## Before/After Summary

| Endpoint | Before | After | Improvement |
|---|---|---|---|
| GET /VitalSigns/critical | 300 rows, no WHERE | 7 rows, WHERE + MAX subquery | 97% fewer rows |
| GET /VitalSigns | 300 rows, no PatientName | 300 rows + JOIN, PatientName included | Client N+1 eliminated |

---

## New DTO
Added `VitalSignWithPatientResponse` — extends the existing response
shape with `PatientName: string`, making the list endpoint self-contained
without requiring a second API call per row.

## Test Suite: 26/26 passing
No regressions — the integration tests use SQLite in-memory which
correctly handles the correlated subquery pattern.

## Key Takeaway
EF Core's ability to translate LINQ to SQL has limits that only appear
at runtime, not at compile time. Always run the endpoint with query
logging enabled after writing a new LINQ query — seeing the generated
SQL is the only reliable way to confirm the translation succeeded and
produced what you intended.
