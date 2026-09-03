# Sprint 2 Retrospective

**Sprint:** Week 7 — Identity & Role-Based Authorization  
**Duration:** 5 days  
**Outcome:** All 13 planned items completed. 26/26 tests passing.

---

## What Went Well

**1. Domain-appropriate role model from the start**  
Choosing Nurse/Doctor/Auditor/Admin instead of generic User/Admin meant every RBAC decision had a real clinical justification. This made the Postman demo significantly more compelling — "a Nurse cannot prescribe medication" is immediately intuitive to any reviewer; "a User cannot access Admin endpoints" is not.

**2. The staffProfileId claim eliminated a whole class of N+1 queries**  
Embedding the domain entity ID in the JWT at login time meant ownership checks in `VitalSignsController.GetById` needed zero additional database lookups — the claim was just there. This wasn't the initial plan; it emerged from thinking through the ownership problem, which is exactly the kind of design insight that justifies building things incrementally.

**3. AuditLoggingMiddleware covered a real gap**  
The decision to place it *after* `UseAuthentication()` but log *after* `await _next(context)` — so claims are resolved and status codes are set before logging — was a genuine technical decision, not just following a template. That detail is exactly what a mentor is likely to ask about.

**4. Running the full test suite after every change caught regressions immediately**  
The `RecordReadingAsync` contract change would have been a silent production bug without the existing unit tests catching it at compile time. The Retrospective from Sprint 1 (explicit banding of "any Contract change must include a test update Backlog item") was directly applied here.

---

## What to Improve

**1. Branch naming didn't match the work scope**  
The Sprint 2 branch is named `week7/day2-auth`, but it contains Day 1 through Day 4 work. The branch should have been named `feature/week7-sprint2` or `sprint2/identity-rbac` from the start. This is a minor but visible inconsistency in the Git history.

**2. The rate limiter interaction with integration tests was a surprise**  
The `429 TooManyRequests` failure in `RoleBasedAccessTests` was not anticipated during Sprint planning. A note that "integration tests that call the login endpoint multiple times may hit rate limits" should have been in the test setup guide from Day 2 when the rate limiter was wired up. The fix (replacing `RateLimiterOptions` in the factory) is correct, but the debugging time (multiple failed attempts before finding the right approach) could have been avoided.

**3. Audit logs have no persistence layer**  
Logging to the console is fine for development but the Sprint 3 backlog item was created *reactively* (discovered during close-out) rather than *proactively* (written as a known gap during Sprint 2 planning). Infrastructure decisions like "where do audit logs go in production?" should be in the backlog before the sprint starts.

---

## One Concrete Action for Sprint 3

**Before writing any Sprint 3 code:** spend 30 minutes mapping every Controller action to a row in the Sprint 3 acceptance criteria table, and for each row explicitly write: "if this endpoint's authorization rule changes, which test file must be updated?" — then link that test file name directly in the backlog item. This prevents the pattern that happened in Sprint 2 where `VitalSignsApiTests` was silently testing a rule that no longer existed.

---

## Metrics

| Metric | Value |
|---|---|
| Planned items | 13 |
| Completed | 13 |
| Tests at sprint start | 24 |
| Tests at sprint end | 26 |
| New tests added | 2 (integration) + 1 (unit) |
| Bugs found in own code | 2 (open `assign-role` endpoint; `RecordReadingAsync` contract break) |
| Bugs found in test code | 3 (missing `department`, rate limiter, stale RBAC assertion) |
| Unresolved edge cases → Sprint 3 | 5 (tagged `[auth-edge]` / `[audit]` / `[ownership]`) |
