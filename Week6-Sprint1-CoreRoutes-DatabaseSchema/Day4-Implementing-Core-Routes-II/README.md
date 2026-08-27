# Day 4 — Core Routes II: Write Operations & Business Logic; Mentor Code Review

### Week 6 · Phase 3, Sprint 1

---

## What I Did

Extended `VitalSignService.RecordReadingAsync` with real business logic beyond
simple field mapping: a Critical reading automatically schedules an urgent
follow-up appointment, unless the patient already has one coming up within 24
hours (avoids duplicate urgent appointments from multiple critical readings in
quick succession).

Wrapped the vital-sign creation and the conditional appointment creation in a
single database transaction (`_context.Database.BeginTransactionAsync()`) — both
writes succeed together or roll back together, since a reading saved without its
required follow-up (or vice versa) would be inconsistent, half-finished data.

## Verification

- Manually confirmed via Swagger: a Critical reading for a patient with no
  upcoming appointment creates a follow-up appointment automatically.
- Full test suite re-run after the change to confirm no regressions.

## Files in This Folder

Read-only snapshot of `VitalSignService.cs` for review. Real file:
`Cardiac-Monitoring-System/src/CardiacMonitoring.Api/Services/VitalSignService.cs`

## Key Takeaway

A multi-step write operation with no transaction boundary can leave the database
in a half-finished, inconsistent state the moment any single step fails —
wrapping related writes in a transaction is what guarantees all-or-nothing behavior.