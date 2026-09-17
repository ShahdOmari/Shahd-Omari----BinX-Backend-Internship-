# Sprint 4 — Definition of Done Audit

## Acceptance Criteria Check

| # | Item | Criteria | Status | Evidence |
|---|---|---|---|---|
| 1 | Test coverage — Auth error paths | Login wrong password → 401, Duplicate register → 400 | ✅ Done | CoverageGapTests.cs |
| 2 | Test coverage — VitalSigns/critical RBAC | Nurse → 403, Admin → 200 | ✅ Done | CoverageGapTests.cs |
| 3 | Test coverage — DELETE /Patients RBAC | Nurse → 403 | ✅ Done | CoverageGapTests.cs |
| 4 | Test coverage — POST /VitalSigns happy path | Returns 201 with correct body | ✅ Done | CoverageGapTests.cs |
| 5 | Test coverage — Ownership check | Nurse B cannot read Nurse A reading → 403 | ✅ Done | CoverageGapTests.cs |
| 6 | Test suite green | 34/34 passing | ✅ Done | dotnet test output |
| 7 | Swagger enriched | Summaries, remarks, examples, response codes on all endpoints | ✅ Done | http://localhost:5286/swagger |
| 8 | XML doc comments | All controllers documented | ✅ Done | Controllers/*.cs |
| 9 | Project README complete | Setup, migrations, env vars, tech stack, architecture | ✅ Done | Cardiac-Monitoring-System/README.md |
| 10 | CI pipeline | Build + test on every push to main and every PR | ✅ Done | GitHub Actions — 2 green runs |
| 11 | CI red-green demo | Deliberate break → ❌, fix → ✅ confirmed | ✅ Done | GitHub Actions history |
| 12 | Live deployment | API reachable at public Railway URL | ✅ Done | Railway dashboard — Deployment successful |
| 13 | Production secrets | JWT key, DB connection via Railway Variables — never in source | ✅ Done | Railway Variables tab |
| 14 | Health check endpoint | GET /health returns 200 | ✅ Done | Railway healthcheck passing |
| 15 | Sprint 3 retrospective action | Query-count gap closed via new RBAC + ownership tests | ✅ Done | CoverageGapTests.cs |

## All 15 items: DONE ✅

---

## Sprint 5 Backlog — Remaining Items

| Tag | Item | Why Deferred |
|---|---|---|
| `[cache]` | Redis SCAN to invalidate all `patients:*` key variants | Requires IConnectionMultiplexer directly |
| `[test]` | Automated query-count regression test for GetCriticalPatients | Needs EF Core diagnostic interceptor — Sprint 5 |
| `[perf]` | Pagination on GET /VitalSigns | Breaking change to response shape |
| `[docs]` | Postman collection final pass — test scripts on every request | Time constraint |
| `[deploy]` | Custom domain on Railway | Nice-to-have, not required for review |
