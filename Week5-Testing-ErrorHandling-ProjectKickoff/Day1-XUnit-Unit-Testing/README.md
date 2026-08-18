# Day 1 — Choosing the Capstone Project & Unit Testing with xUnit

### Week 5 · BinX Backend Development Internship (.NET)

---

## 1. Learning Objectives

- Choose a Phase 3 capstone project and confirm it can meet the professional baseline.
- Write unit tests using xUnit's `[Fact]` and `[Theory]` attributes.
- Apply the Arrange-Act-Assert pattern consistently across tests.

## 2. Project Chosen

**Cardiac Patient Monitoring System** — already built end-to-end across Weeks 1-4
(entities, generic repository, EF Core, Identity/JWT auth with Nurse/Doctor roles,
FluentValidation, hardening). It naturally supports the professional baseline:
a documented REST API, a normalized schema with migrations, JWT-based RBAC, and now
a growing test suite.

## 3. What I Built

- Set up a dedicated `CardiacMonitoring.Tests` xUnit project, referencing the API
  project directly.
- Wrote unit tests for `CardiacRiskEvaluator` — the highest-value target in the
  codebase for testing, since it is pure, dependency-free branching logic that
  directly decides a patient's risk classification.
- 4 `[Fact]` tests covering the obvious cases (Normal, Critical via low oxygen,
  Critical via high heart rate, Watch).
- 1 `[Theory]` test with 6 `[InlineData]` cases covering every heart-rate threshold
  boundary exactly and one value on each side of it — this is the kind of test most
  likely to catch a real regression if a threshold constant is ever changed by
  accident later.

## 4. Result

**10 / 10 tests passing.** All boundary values behaved exactly as the evaluator's
`>` / `<` comparisons intended.

## 5. Note on This Folder

This folder contains a **read-only snapshot** of the test file written this day, for
review purposes. The actual, buildable, currently-maintained version of this file
lives in the real project at:

Cardiac-Monitoring-System/tests/CardiacMonitoring.Tests/Services/CardiacRiskEvaluatorTests.cs 

Run `dotnet test` from `Cardiac-Monitoring-System/` to execute the full suite.

## 6. Key Takeaway

Testing priority should follow risk and complexity, not ease — `CardiacRiskEvaluator`
was tested first specifically because it is the piece of business logic most capable
of silently doing the wrong thing without anyone noticing.
