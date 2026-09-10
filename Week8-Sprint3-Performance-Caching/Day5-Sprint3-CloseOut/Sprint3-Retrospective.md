# Sprint 3 Retrospective

**Sprint:** Week 8 — Advanced Queries & Performance
**Duration:** 5 days
**Outcome:** All 11 planned items completed. 26/26 tests passing throughout.

---

## What Went Well

**1. Query logging made the problem undeniable**
Before enabling `.LogTo(Console.WriteLine)`, GetCriticalPatients looked
fine — it returned the right data and passed all tests. The SQL log showed
`SELECT * FROM VitalSigns` with no WHERE clause and made the problem
impossible to argue with. Every performance claim this sprint was backed by
actual terminal output, not a guess or a benchmark tool.

**2. Realistic seed data changed what was visible**
With 2 patients, a full table load runs in under 1ms and looks perfectly
acceptable. With 300 readings the problem is measurable; with 300,000 it
would be a production outage. Seeding 300 readings before diagnosing was
the correct sequence — the diagnosis would have been invisible otherwise.

**3. The ICacheService abstraction proved its value immediately**
When the integration tests broke because Redis was not available in the
test environment, the fix was a one-line change in CardiacApiFactory:
replace the Redis-backed IDistributedCache with AddDistributedMemoryCache().
No controller code changed. If ICacheService had been IDistributedCache
called directly in PatientsController, the fix would have required changing
every call site.

**4. EF Core limitation caught at runtime, not at review**
GroupBy().Select(g => g.First()) compiles cleanly in C# but throws a
KeyNotFoundException when EF Core tries to translate it to SQL. Documenting
this as a known EF Core 10 limitation — not a bug in the code, but a
boundary of what the ORM can translate — is more useful than just fixing it.

---

## What to Improve

**1. First-call cache latency was unexpectedly high**
The first GET /Patients after server start took 2651ms — partly because
Redis needed to warm up, partly because the SQL Server connection pool was
cold. This is a known cold-start pattern, but it was not anticipated before
the measurement. A cache warm-up strategy (pre-populating the cache on
startup) should be evaluated for Sprint 4.

**2. Cache invalidation only covers the default key**
POST /Patients invalidates `patients:page=1&size=10&...` — the default
page. A Doctor browsing page 2, or filtering by Female patients, has a
cached response that is now stale until it expires. The correct fix is a
Redis SCAN with pattern `patients:*` via IConnectionMultiplexer, which
was identified but deferred to Sprint 4 to keep this sprint's scope tight.

**3. No automated regression test for query count**
The correlated subquery fix is confirmed manually by reading the terminal.
If a future developer reverts GetCriticalPatients to GetAllAsync() by
accident, no test will catch it — the endpoint still returns correct data,
just slowly. A test asserting query count ≤ 2 would make this a permanent
safeguard.

---

## One Concrete Action for Sprint 4

**Before writing any Sprint 4 code:** write one integration test that
intercepts EF Core's query log and asserts that `GET /VitalSigns/critical`
executes exactly 1 SQL statement. This turns the most impactful fix of
Sprint 3 into a permanent regression guard — the next developer who
accidentally replaces the correlated subquery with GetAllAsync() will see
a test failure, not a slow endpoint in production.

---

## Metrics

| Metric | Value |
|---|---|
| Planned items | 11 |
| Completed | 11 |
| Tests at sprint start | 26 |
| Tests at sprint end | 26 |
| Regressions introduced | 1 (ICacheService not registered → fixed same day) |
| EF Core limitations hit | 1 (GroupBy+First cannot be translated) |
| Performance items → Sprint 4 | 6 (tagged) |

---

## Performance Summary

| Change | Before | After | Evidence |
|---|---|---|---|
| GetCriticalPatients rows | 300 | 7 | Terminal SQL log |
| GET /Patients cache miss | ~45ms | ~30ms | Terminal AUDIT log |
| GET /Patients cache hit | ~45ms | 3ms | Terminal AUDIT log |
| VitalSigns/critical cold | — | 387ms | Terminal AUDIT log |
| VitalSigns/critical warm | — | 50ms | Terminal AUDIT log |
