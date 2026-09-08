# Day 3 — Redis Caching with Cache-Aside Pattern

## Sprint Context
Sprint 3, Week 8, Day 3.

## What Was Built

### Redis Setup
- Memurai (Redis-compatible, Windows-native) running on `localhost:6379`
- `Microsoft.Extensions.Caching.StackExchangeRedis` package added
- `IDistributedCache` registered in `Program.cs` backed by Redis
- Connection string added to `appsettings.Development.json`

### ICacheService — Typed Cache Abstraction
Rather than scattering raw `IDistributedCache` calls across controllers,
a typed `ICacheService` wrapper centralises serialization, expiry, and
error handling. If Redis goes down, every method catches the exception
and logs a warning — the request falls through to the database rather
than returning 500. A cache that takes the API down with it is worse
than no cache at all.

### CacheKeys — Centralised Key Definitions
All cache keys defined in one static class so a typo in one controller
cannot silently create a key that another controller never invalidates.
Keys include all query parameters so different filter combinations are
cached independently:
patients:page=1&size=10&gender=&minAge=&sort=name&dir=asc
patients:page=1&size=10&gender=Female&minAge=40&sort=name&dir=asc

### Cache-Aside Pattern on GET /Patients
Request → Check cache
├── HIT → return immediately (no DB query)
└── MISS → query DB → store in cache → return result

POST/PUT/DELETE /Patients → write to DB → CACHE DEL → next GET = fresh data

**Why GET /Patients:** Read on every shift by every Nurse and Doctor.
Patient roster changes rarely compared to how often it is read.
VitalSigns were not cached because new readings arrive every few hours.

**Expiry:** 10 minutes — long enough to absorb read spikes; short
enough that a missed invalidation self-heals within a predictable window.

---

## Measured Results (real terminal output)

### Cache Miss → Cache Hit progression
CACHE MISS → 2651ms (first request: cold DB + Redis SET)
CACHE HIT → 52ms
CACHE HIT → 9ms
CACHE HIT → 3ms
**Steady-state improvement: ~98% faster (2651ms → 3ms)**

### Cache Invalidation confirmed
POST /api/v1/Patients → INSERT INTO [Patients] → CACHE DEL
GET /api/v1/Patients → CACHE MISS → SELECT COUNT() + SELECT [p]. → CACHE SET → 30ms
GET /api/v1/Patients → CACHE HIT → 4ms
GET /api/v1/Patients → CACHE HIT → 2ms
New patient appeared immediately on the next GET — zero stale data.

---

## Test Fix: CardiacApiFactory
Integration tests run without Redis. The factory now:
- Removes Redis `IDistributedCache` descriptors
- Registers `AddDistributedMemoryCache()` (in-memory, no Redis needed)
- Re-registers `ICacheService` against the in-memory cache

Tests verify endpoint correctness, not caching — Redis being unavailable
in the test environment must never cause a 500 on any endpoint that
injects `ICacheService`.

## Test Suite: 26/26 passing
