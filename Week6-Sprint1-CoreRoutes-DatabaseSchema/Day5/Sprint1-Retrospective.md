# Sprint 1 Retrospective

### Cardiac Patient Monitoring System — Phase 3, Sprint 1

---

## What Went Well

- Choosing a real business rule (auto-scheduled follow-up on a Critical reading)
  instead of a generic CRUD write operation made the transaction requirement
  genuinely necessary, not just an exercise — the transaction actually protects
  against a real inconsistent-data scenario.
- The existing Week 1-5 test suite caught the exact regression risk introduced by
  Day 4's refactor immediately (`VitalSignService`'s constructor signature change)
  — a broken build was caught by `dotnet test` before it ever reached a pull
  request, not discovered later in review.

## What Didn't Go Well

- Refactoring `VitalSignService` to support a real database transaction forced
  changing the existing Moq-based unit tests to a hybrid approach (real SQLite
  `AppDbContext` + mocked `IRiskEvaluator`), since a transaction can't be
  meaningfully mocked. This wasn't planned for at Sprint Planning — the backlog
  sizing for Day 4 didn't account for updating existing tests, only writing new
  business logic.

## One Concrete Action for Sprint 2

When sizing any backlog task that changes a class's constructor or public
contract, explicitly include "update existing tests referencing this class" as
its own line item in the backlog — not an implicit assumption folded into the
main task's estimate.
