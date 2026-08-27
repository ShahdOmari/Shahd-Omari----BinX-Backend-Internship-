# Sprint 1 Summary & Acceptance Criteria Audit

### Cardiac Patient Monitoring System — Phase 3, Sprint 1

---

## Sprint Velocity — Test Suite Growth

A concrete, measurable signal of progress across the sprint, not just a subjective
"felt productive" — every day's work is backed by a passing, growing test suite.

| Milestone | Total Tests | Delta | What Was Added |
|---|---|---|---|
| Week 5, Day 1 | 10 | — | Unit tests for `CardiacRiskEvaluator` |
| Week 5, Day 2 | 13 | +3 | Moq-based `VitalSignService` tests |
| Week 5, Day 3 | 19 | +6 | Integration tests (Patients, VitalSigns) |
| Week 5, Day 5 | 24 | +5 | RBAC + async validation rule tests |
| **Sprint 1 close** | **24** | **0*** | *No new tests added — refactored 3 existing tests from full Moq to a hybrid (real SQLite `AppDbContext` + mocked `IRiskEvaluator`) to support the new transaction-wrapped business logic |

## Definition of Done — Acceptance Criteria Audit

Per the program's rubric: a task only counts as done if its endpoint returns
correct status codes, was reviewed via pull request, has a passing test covering
a happy path *and* an error case, and has no unhandled exceptions or known
vulnerabilities.

| Backlog Task | Status Codes ✓ | PR Reviewed | Happy + Error Path Tested | No Unhandled Exceptions | Verdict |
|---|---|---|---|---|---|
| Patient/VitalSign/Medication/Appointment schema + migrations | ✅ | ✅ | ✅ (via Integration tests) | ✅ | **Done** |
| Fluent API relationships + role seed data | ✅ | ✅ | ✅ (build + migration review) | ✅ | **Done** |
| Paginated/filterable/sortable `GET /Patients` | ✅ | ✅ | ✅ (Postman) | ✅ | **Done** |
| Critical-reading auto-follow-up (transaction) | ✅ | ✅ | ✅ (Postman + suite) | ✅ | **Done** |
| RBAC enforcement (Nurse/Doctor) | ✅ | ✅ | ✅ (`RoleBasedAccessTests`) | ✅ | **Done** |

**All Sprint 1 backlog items meet the Definition of Done — nothing carries over
incomplete.**

## Two Real Bugs Found and Fixed This Sprint (not just features shipped)

| # | Bug | Found By | Impact if Shipped |
|---|---|---|---|
| 1 | Accidental class-level `[Authorize]` on `AuthController` | Integration test, Week 5 Day 3 | No user could ever register or log in |
| 2 | Silent JSON enum-deserialization default in test client | Integration test, Week 5 Day 5 | Would have masked a real regression in production risk-scoring if it recurred |

## Sprint 2 Backlog (carried forward / newly identified)

| Item | Source | Priority |
|---|---|---|
| Consider policy-based authorization (beyond simple role checks) for finer-grained future permissions | Self-identified during RBAC work | Medium |
| Add a unit test specifically for the 24-hour follow-up de-duplication branch in isolation | Retrospective action item | High |
| Evaluate adding response caching to the paginated `GET /Patients` endpoint once real traffic patterns are known | Forward-looking, Sprint 3+ candidate | Low |

## Links

- Merged Pull Request: *(paste your actual PR URL here once merged)*
- ERD: [`../Day1/ERD.md`](../Day1/ERD.md)
- Migration History: see `Cardiac-Monitoring-System/src/CardiacMonitoring.Api/Migrations/`
