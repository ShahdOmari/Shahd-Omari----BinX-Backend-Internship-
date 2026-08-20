# Day 5 — Applying Testing to the Chosen Project; Week 5 Synthesis

### Week 5 · BinX Backend Development Internship (.NET)

---

## 1. Learning Objectives

- Prioritize what to test first on a growing codebase, based on risk and
  complexity — not ease.
- Run the full test suite and interpret its results.
- Understand how this week's foundation carries directly into Phase 3's sprint
  structure.

## 2. Risk Analysis — What Needed Testing Most

Reviewing the entire capstone project (Weeks 1-4's work) against real risk rather
than ease of testing, three areas stood out as the highest priority:

| # | Area | Why It's High-Risk | Status Before Today |
|---|---|---|---|
| 1 | `CardiacRiskEvaluator` | Real branching medical-classification logic — a bug here means a critical patient could silently show as Normal | ✅ Already covered (Day 1) |
| 2 | **Role-based access control** (Doctor-only actions) | A regression here already happened once (Day 3's accidental class-level `[Authorize]` bug) and could let an unprivileged Nurse perform Doctor-only actions like prescribing or removing medication | ❌ **Gap — addressed today** |
| 3 | **Async FluentValidation business rules** (does the referenced `PatientId` actually exist?) | A silent regression would let orphaned `VitalSign`/`Medication`/`Appointment` records reach the database with no real patient behind them | ❌ **Gap — addressed today** |

Two genuine gaps were identified and closed today, rather than adding more tests to
areas already well-covered — following the lesson's explicit guidance to prioritize
by risk, not chase blanket coverage.

## 3. What I Built

- **`RoleBasedAccessTests.cs`** — 3 tests exercising the RBAC boundary end-to-end
  with real issued JWTs for both roles: a Nurse is correctly rejected (`403`) from
  creating and deleting medication, while a Doctor succeeds at both. Distinguishes
  `403 Forbidden` (authenticated, not permitted) from `401 Unauthorized`
  (not authenticated) deliberately, since confusing the two is a common, subtle bug.
- **`BusinessRuleValidationTests.cs`** — 2 tests confirming the async
  `PatientId`-existence rule actually blocks invalid references (`400` with a clear
  message) and allows valid ones through (`201`), with a bonus end-to-end check that
  the risk-scoring feature also produces the correct classification when exercised
  through the real HTTP pipeline, not just in isolation.

## 4. A Second Real Issue Found — and a Genuine Root-Cause Fix

`CreateVitalSign_ReturnsCreated_WhenPatientIdIsValid` initially failed with
`Expected: Critical, Actual: Normal` — but only when reading the response back
through the test client, never in the API itself.

**Diagnosis process:** rather than guessing, the raw HTTP response body was logged
directly, which showed the API had in fact returned `"riskLevel":"Critical"`
correctly. The bug was isolated entirely to the test's own deserialization: a bare
`new JsonSerializerOptions()` with a `JsonStringEnumConverter` added manually does
not enable case-insensitive property name matching, so `System.Text.Json` silently
left every property at its default value (`RiskLevel` defaulting to `0`, i.e.
`Normal`) instead of throwing — the worst kind of bug, one that fails silently rather
than loudly.

**Fix:** confirmed via a temporary diagnostic (logging the raw response body directly before deserializing) that the API's JSON was correct, isolating the bug entirely to the test client's deserialization settings. Replaced the bare
`new JsonSerializerOptions()` with `new JsonSerializerOptions(JsonSerializerDefaults.Web)`
as the base — matching ASP.NET Core's own default JSON behavior — before adding the
`JsonStringEnumConverter`. All 24 tests pass after the fix, confirmed by re-running
the full suite (`dotnet test`), not just the one previously failing test.

This is a genuinely useful lesson in its own right: a silent default-value fallback
in a deserializer can produce a passing-looking assertion failure that has nothing to do with the system under test — the raw response is always worth checking directly before assuming the API itself is wrong.

## 5. Full Test Suite Result

dotnet test


**Total: 24 tests — 24 passed, 0 failed.**

| Source | Tests |
|---|---|
| Day 1 — `CardiacRiskEvaluatorTests` | 10 |
| Day 2 — `VitalSignServiceTests` | 3 |
| Day 3 — `PatientsApiTests`, `VitalSignsApiTests` | 6 |
| Day 5 — `RoleBasedAccessTests`, `BusinessRuleValidationTests` | 5 |
| **Total** | **24** |

## 6. Note on This Folder

This folder contains a **read-only snapshot** of the files written this day, for
review purposes. The actual, buildable, currently-maintained versions live in the
real project at:  
Cardiac-Monitoring-System/tests/CardiacMonitoring.Tests/Integration/RoleBasedAccessTests.cs
Cardiac-Monitoring-System/tests/CardiacMonitoring.Tests/Integration/BusinessRuleValidationTests.cs

Run `dotnet test` from `Cardiac-Monitoring-System/` to execute the full suite.

## 7. Previewing Phase 3

Starting next week, work moves into four one-week sprints (Sprint Planning → daily
stand-ups → mid-sprint review → Sprint Review/Retrospective), taking this same
capstone project from its current baseline through full authentication/RBAC
hardening (already largely in place), performance work, and finally deployment.
Everything built this week — the testing discipline, the risk-first prioritization,
the centralized error handling — is the standard every future sprint is expected to
maintain, not a one-off Week 5 exercise.

## 8. Key Takeaway

Testing priority should follow risk, not convenience. The two gaps closed today
(RBAC enforcement, async validation rules) were chosen specifically because a silent
failure in either would have real consequences — unauthorized medication changes or
orphaned clinical records — while simple pass-through code elsewhere in the project
was correctly left untested, exactly as the lesson advises.
