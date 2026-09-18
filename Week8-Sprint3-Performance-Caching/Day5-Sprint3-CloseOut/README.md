# Day 5 — Sprint 3 Close-Out

## Sprint Context
Sprint 3, Week 8, Day 5 — final day.

## What Was Done

### 1. Demo (Postman + Terminal)
Full before/after performance evidence presented across 4 scenes:
- Full table load diagnosis (SELECT * no WHERE, 300 rows)
- Correlated subquery fix (7 rows, 97% reduction)
- Redis cache hit/miss timing (2651ms → 3ms, 98% faster)
- Database index effect (table scan → index seek, warm query 50ms)

See `Demo-Script.md` for full narrated walkthrough.

### 2. Definition of Done Audit
All 11 Sprint 3 items verified — all complete. See `Sprint3-DoD-Audit.md`.

### 3. Sprint 4 Backlog — Edge Cases Tagged
6 items deferred from Sprint 3:
- `[cache]` Redis SCAN for full key namespace invalidation
- `[cache]` Cache warm-up on startup
- `[index]` Appointments indexes
- `[perf]` Pagination on GET /VitalSigns
- `[test]` Automated query-count regression test
- `[cache]` Cache hit/miss diagnostics endpoint

### 4. Sprint Retrospective
See `Sprint3-Retrospective.md`.

### 5. Sprint Summary
See `Sprint3-Summary.md` — all before/after measurements, caching strategy, PR link.

## Performance Results (measured, not estimated)

| Change | Before | After |
|---|---|---|
| GetCriticalPatients rows transferred | 300 | 7 (-97%) |
| GET /Patients cache hit | ~45ms | 3ms (-98%) |
| VitalSigns/critical warm query | table scan | 50ms index seek |

## Key Takeaway
Every performance claim this sprint is backed by real terminal output.
The pattern: push work to the layer designed for it —
filtering in SQL, caching repeated reads, indexing searched columns.

## Test Suite: 26/26 passing
