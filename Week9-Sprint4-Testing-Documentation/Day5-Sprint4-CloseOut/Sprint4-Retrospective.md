# Sprint 4 Retrospective

**Sprint:** Week 9 — Testing & Documentation
**Duration:** 5 days
**Outcome:** All 15 DoD items complete. 34/34 tests. Live API on Railway. CI pipeline green.

---

## What Went Well

**1. Test factory design paid off immediately in CI**
The decision in Sprint 3 to replace Redis with AddDistributedMemoryCache()
in CardiacApiFactory meant the CI pipeline worked on the first push with
zero configuration — no service containers, no secrets, no setup scripts.
A test suite that requires external services in CI is a test suite that
breaks randomly; designing against abstractions prevented that entirely.

**2. Closing coverage gaps surfaced a real serialization mismatch**
When adding tests that deserialize VitalSignResponse, they failed because
the API serializes RiskLevel as a string ("Critical") via JsonStringEnumConverter
but the test client used default deserializer options. This was a genuine
production-visible discrepancy — any client not configuring the same
JsonStringEnumConverter would get a deserialization error. Fixed by reading
the ID via JsonDocument.Parse() rather than full deserialization, and
documented as a known client-side integration note.

**3. XML doc comments improved Swagger from a bare route list to a usable reference**
Before Day 2, Swagger showed endpoints with no context. After adding
summaries, remarks, example request bodies, and ProducesResponseType
attributes, the Swagger UI became self-documenting — a new developer
can understand the ownership model, the role requirements, and the
cache behaviour without reading a single line of controller source code.

**4. Railway deployment with zero downtime configuration**
The multi-stage Dockerfile (SDK image for build, ASP.NET runtime image for
deploy) kept the production container small. The railway.json encoding bug
(invalid character Ï) was caught and fixed in one commit — writing ASCII
explicitly rather than relying on PowerShell's default UTF-8 with BOM.

---

## What to Improve

**1. The CI deploy job is not yet fully wired**
The deploy job in ci.yml references `railway up` but the RAILWAY_TOKEN
secret was not added to GitHub Secrets — the deploy step is configured
but not yet executing automatically. Manual deployment via Railway's
GitHub integration works, but the fully automated gate (test → deploy
in one pipeline) is partially complete.

**2. Test coverage audit was reactive, not planned**
The coverage gaps (ownership check, VitalSigns/critical RBAC, Auth error
paths) were discovered on Day 1 of Sprint 4 by auditing the endpoint list
against the test files. These gaps existed since Sprint 2 — a coverage
audit at the end of each sprint would have caught them earlier rather than
accumulating them into a single Sprint 4 cleanup pass.

**3. Postman collection was not finalized**
The lab requires one test script per endpoint minimum. The collection was
not given a final pass to confirm every endpoint has a test script. This
is the most visible documentation gap going into Week 10.

---

## One Concrete Action for Week 10

**Before the presentation:** add the RAILWAY_TOKEN secret to GitHub Actions
secrets so the full CI/CD pipeline (build → test → deploy) runs end-to-end
automatically on every push to main. This turns the current semi-automated
setup into a fully automated delivery pipeline — exactly what Week 10's
final review evaluates.

---

## Metrics

| Metric | Value |
|---|---|
| Planned DoD items | 15 |
| Completed | 15 |
| Tests at sprint start | 26 |
| Tests at sprint end | 34 |
| New tests added | 8 |
| Coverage gaps closed | 5 endpoint categories |
| CI pipeline runs | Green ✅ |
| Deployment status | Live on Railway ✅ |
| Deferred to backlog | 5 items tagged |
