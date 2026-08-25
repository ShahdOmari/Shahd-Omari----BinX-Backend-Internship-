# Day 3 — Implementing Core Routes I: Catalog & Read Operations

### Week 6 · Phase 3, Sprint 1

---

## Learning Objectives

- Implement paginated list endpoints rather than returning entire tables at once.
- Support filtering and sorting through query parameters.
- Project entities to DTOs rather than exposing them directly.

## What I Did

Rebuilt `GET /api/v1/Patients` as a real catalog-style endpoint instead of the
original "return everything" version:

- **Pagination** — `page` and `pageSize` query parameters, applied via LINQ's
  `Skip`/`Take` after filtering and sorting. Defensive bounds reject an invalid
  page (< 1) or an oversized `pageSize` (> 100), falling back to safe defaults
  instead of letting a client force the query to return everything.
- **Filtering** — two optional query parameters, `gender` and `minAge`, each
  applied conditionally (only when actually supplied), so the same endpoint
  serves both an unfiltered browse and a narrowed search.
- **Sorting** — `sortBy` (`name` or `age`) combined with `sortDirection`
  (`asc`/`desc`), matched explicitly with a `switch` expression rather than
  accepting an arbitrary raw column name from the client.
- **DTO projection** — added a generic `PagedResult<T>` response wrapper
  (`Items`, `Page`, `PageSize`, `TotalCount`) so any future list endpoint can
  reuse the same shape. The query projects directly to `PatientResponse` via
  `.Select(...)` before materializing, so EF Core only selects the columns
  actually needed instead of loading full entities into memory first.
- Extended the generic repository with a `Query()` method returning
  `IQueryable<T>`, since the existing `GetAllAsync()` couldn't support
  filtering/sorting/pagination without pulling the entire table into memory first.

## Verification

Tested in Postman (`Sprint1-Day3-Catalog` folder in the collection) across four
combinations:

| Request | What It Confirms |
|---|---|
| Default call | Response shape matches `PagedResult<T>` (`items`, `page`, `pageSize`, `totalCount`) |
| `pageSize=1` | Pagination actually limits the returned item count |
| `gender=Female` | Filter is applied correctly and only matching records return |
| `sortBy=name&sortDirection=desc` | Sorting is applied correctly and consistently |

All four passed.

## Files in This Folder

- Read-only snapshots of the changed files, for review. The real, buildable code
  lives at:
  - `Cardiac-Monitoring-System/src/CardiacMonitoring.Api/DTOs/Common/PagedResult.cs`
  - `Cardiac-Monitoring-System/src/CardiacMonitoring.Api/Repositories/IRepository.cs`
  - `Cardiac-Monitoring-System/src/CardiacMonitoring.Api/Repositories/Repository.cs`
  - `Cardiac-Monitoring-System/src/CardiacMonitoring.Api/Controllers/PatientsController.cs`

## Key Takeaway

An unpaginated "list everything" endpoint is one of the most common causes of a
slow, unresponsive API once real data volume arrives — pagination belongs in a
list endpoint from the start, not added later as an optimization once it becomes
a problem.