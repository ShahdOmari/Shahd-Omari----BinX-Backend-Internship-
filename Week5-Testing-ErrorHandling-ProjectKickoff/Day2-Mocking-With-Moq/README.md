# Day 2 — Mocking Dependencies with Moq

### Week 5 · BinX Backend Development Internship (.NET)

---

## 1. Learning Objectives

- Explain why a unit test should isolate its target from real dependencies.
- Set up and configure mocks with Moq.
- Verify mock interactions (call counts, arguments) rather than just return values.

## 2. What I Built

**Refactor first:** the "record a reading and score it" logic was previously
embedded directly inside `VitalSignsController.Create`. Extracted it into a
dedicated `VitalSignService` (`IVitalSignService` / `VitalSignService`), depending
on `IRepository<VitalSign>` and `IRiskEvaluator` — both already interfaces, thanks
to Week 2's dependency-injection discipline. This is a genuine architectural
improvement (separating business logic from HTTP concerns), not just test scaffolding.

**Then, tests:** `VitalSignServiceTests.cs` mocks both dependencies with Moq, so the
service's own coordination logic is tested with zero real database or scoring calls:

- `RecordReadingAsync_AssignsRiskLevel_FromEvaluator` — confirms the service uses
  whatever the (mocked) evaluator returns.
- `RecordReadingAsync_SavesExactlyOnce` — uses `Mock.Verify(..., Times.Once)` to
  confirm `AddAsync` and `SaveChangesAsync` are each called exactly once, not zero
  or twice.
- `RecordReadingAsync_PassesCorrectPatientIdToRepository` — captures the actual
  entity passed to the mocked repository via a `Callback`, confirming the right
  data reaches persistence.

## 3. Result

**3 / 3 new tests passing** (13/13 total including Day 1).

## 4. Note on This Folder

This folder contains a **read-only snapshot** of the files touched this day, for
review purposes. The actual, buildable, currently-maintained versions live in the
real project at:
Cardiac-Monitoring-System/src/CardiacMonitoring.Api/Services/IVitalSignService.cs
Cardiac-Monitoring-System/src/CardiacMonitoring.Api/Services/VitalSignService.cs
Cardiac-Monitoring-System/tests/CardiacMonitoring.Tests/Services/VitalSignServiceTests.cs

Run `dotnet test` from `Cardiac-Monitoring-System/` to execute the full suite.

## 5. Key Takeaway

Mocking only works cleanly when dependencies are already interfaces, not concrete
classes — a decision made back in Week 2, paying off directly here. `Verify()` catches
a class of bug a simple return-value assertion can't: a save call silently skipped or
duplicated.
