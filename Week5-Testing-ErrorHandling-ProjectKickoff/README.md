
# Week 5 — Testing, Error Handling & Project Kickoff

### BinX Backend Development Internship (.NET) — Phase 2 → 3

---

## Overview

This week closes Phase 2 and opens Phase 3: writing real unit and integration tests
for the chosen capstone project, and beginning the centralized error-handling work
every endpoint from here forward is expected to meet.

**Capstone project:** Cardiac Patient Monitoring System (`Cardiac-Monitoring-System/`
at the repo root) — the same project this week's testing work is applied directly to.

## Daily Progress

| Day | Topic | Status |
|---|---|---|
| [Day 1](./Day1-XUnit-Unit-Testing) | Project selection + xUnit unit testing | ✅ Complete — 10/10 tests |
| [Day 2](./Day2-Mocking-With-Moq) | Mocking dependencies with Moq | ✅ Complete — 13/13 tests |
| [Day 3](./Day3-Integration-Testing-WebApplicationFactory) | Integration testing with WebApplicationFactory | ✅ Complete — 19/19 tests |
| Day 4 | Centralized error handling & global exception middleware | ⬜ In progress |
| Day 5 | Applying testing to the project; Week 5 synthesis | ⬜ Not started |

## Structure Note

Each day's folder contains a **documentation snapshot** of the relevant files — for
review, not for building or running directly. The single, real, continuously-updated
copy of the project (source + tests) lives at the repo root in
`Cardiac-Monitoring-System/`, and is what actually builds, runs, and is graded.

Cardiac-Monitoring-System/
├── src/CardiacMonitoring.Api/ (the API)
└── tests/CardiacMonitoring.Tests/ (all unit + integration tests)

Run the full suite from there:
```bash
cd Cardiac-Monitoring-System
dotnet test
```

## Test Suite Growth This Week

| After Day | Total Tests | New This Day |
|---|---|---|
| Day 1 | 10 | Unit tests for `CardiacRiskEvaluator` |
| Day 2 | 13 | Moq tests for `VitalSignService` (after extracting it from the controller) |
| Day 3 | 19 | Integration tests via `WebApplicationFactory` + SQLite in-memory |

## Real Bug Caught This Week

Day 3's integration tests caught a genuine authorization bug: `AuthController` had
been accidentally decorated with a class-level `[Authorize]`, which would have
blocked every unauthenticated user from registering or logging in at all. It went
unnoticed in manual Swagger testing purely because a leftover valid token was always
present from a previous session. See the [Day 3 README](./Day3-Integration-Testing-WebApplicationFactory)
for the full writeup.

## Tools Used

xUnit · Moq · Microsoft.AspNetCore.Mvc.Testing (WebApplicationFactory) ·
Microsoft.EntityFrameworkCore.Sqlite

---

*Updated as Days 4-5 are completed.*
