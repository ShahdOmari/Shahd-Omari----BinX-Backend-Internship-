# Day 3 — Integration Testing with WebApplicationFactory

### Week 5 · BinX Backend Development Internship (.NET)

---

## 1. Learning Objectives

- Set up `WebApplicationFactory` to host the API in-memory for testing.
- Write integration tests against real HTTP endpoints.
- Use a separate test database, isolated from the real development database.
- Test an authenticated, `[Authorize]`-protected endpoint with a real issued JWT.

## 2. What I Built

- `CardiacApiFactory` — a custom `WebApplicationFactory<Program>` that swaps the
  real SQL Server `AppDbContext` registration for a SQLite **in-memory** database,
  created fresh for every test run.
- `PatientsApiTests` — the primary resource's Get-by-id happy path (asserting on the
  full response body, not just the status code), the not-found error path, and a
  boundary test confirming the controller rejects requests with no token at all.
- `VitalSignsApiTests` — a dedicated class for the protected-route requirement:
  a real Register → Login → JWT flow attached as a Bearer token succeeds; no token
  is rejected with 401; and a malformed, non-real JWT string is also rejected with
  401 (confirming the API actually validates signatures, not just presence).
- A shared `RegisterAndLoginAsync` helper on the factory, reused across all
  authenticated tests instead of duplicating the register/login HTTP calls
  everywhere.

## 3. A Real Bug Found by These Tests

While wiring up authenticated test clients, `Register` and `Login` unexpectedly
returned `401 Unauthorized` with an empty body — impossible, since neither endpoint
should require a token to reach in the first place.

Root cause: `AuthController` had an `[Authorize]` attribute on the **class level**,
copy-pasted by accident while adding `[Authorize]` to the other controllers earlier
in the week. It went unnoticed during manual Swagger testing only because a valid
token from a previous session was always already sitting in Swagger's Authorize
button. Fixed by removing `[Authorize]` from the class level, leaving only the
route-specific `[EnableRateLimiting]` attributes.

This is a direct, concrete example of why integration tests matter: a bug the manual
testing workflow was structurally blind to was caught immediately once tests
exercised the real HTTP pipeline from a clean, unauthenticated client.

## 4. Technical Note — Choosing SQLite In-Memory

LocalDB (used for manual development) is Windows-only. Since this project is
intended to run in Week 9's CI pipeline (GitHub Actions, which defaults to Linux
runners), a test database had to be cross-platform. SQLite in-memory is fully
portable, fast, and self-cleaning (each `WebApplicationFactory` instance gets a
fresh, isolated database that vanishes when its connection closes) — the correct
choice for a project headed toward CI/CD, not just the easiest one for now.

## 5. Result

**6 / 6 new tests passing** (19/19 total across Days 1-3).

## 6. Note on This Folder

This folder contains a **read-only snapshot** of the files written this day, for
review purposes. The actual, buildable, currently-maintained versions live in the
real project at:
Cardiac-Monitoring-System/tests/CardiacMonitoring.Tests/Integration/CardiacApiFactory.cs
Cardiac-Monitoring-System/tests/CardiacMonitoring.Tests/Integration/PatientsApiTests.cs
Cardiac-Monitoring-System/tests/CardiacMonitoring.Tests/Integration/VitalSignsApiTests.cs


Run `dotnet test` from `Cardiac-Monitoring-System/` to execute the full suite.

## 7. Key Takeaway

Integration tests exercise the real middleware pipeline end-to-end — routing,
authentication, authorization, model binding, all together — which is exactly why
they can catch configuration bugs (like a stray `[Authorize]`) that unit tests,
by design, never will.
