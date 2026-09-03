# Day 3 — Apply RBAC and Ownership Checks

## Sprint Context
Sprint 2 (Identity & Role-Based Authorization), Day 3 of 5.

## What Was Built

### 1. Default role at registration + seeded Admin
- Every `POST /api/v1/Auth/register` call now defaults the new account to
  the **Nurse** role — the most common day-to-day account type (recording
  vitals, viewing patients). Doctor and Admin are privileged roles granted
  explicitly afterward, never self-selected at signup.
- `Program.cs` seeds one initial **Admin** account (`admin@cardiac.com`)
  on startup, idempotently (skipped if the account already exists). This
  solves a chicken-and-egg problem: since `assign-role` is now Admin-only,
  there must always be at least one Admin able to grant further roles.

### 2. Role requirements applied across every endpoint
Real, domain-appropriate roles were used instead of a generic Admin/User
split — **Nurse**, **Doctor**, and **Admin** — reflecting who actually
uses a cardiac monitoring system day to day.

| Controller   | Endpoint              | Access level                      |
|--------------|------------------------|------------------------------------|
| Patients     | GET (all / by id)      | Any authenticated staff           |
| Patients     | POST / PUT             | Any authenticated staff           |
| Patients     | DELETE                 | Doctor, Admin                      |
| VitalSigns   | GET all                | Doctor, Auditor, Admin             |
| VitalSigns   | GET by id              | Owner (Nurse) or Doctor/Admin/Auditor |
| VitalSigns   | POST                   | Any authenticated staff           |
| VitalSigns   | GET critical           | Doctor, Admin                      |
| Medications  | GET (all / by id)      | Any authenticated staff           |
| Medications  | POST / PUT / DELETE    | Doctor, Admin                      |
| Appointments | GET / POST / PUT       | Any authenticated staff           |
| Appointments | DELETE                 | Doctor, Admin *(tightened — previously open to anyone)* |
| Auth         | register / login       | Public                             |
| Auth         | assign-role            | Admin *(tightened — previously open to anyone)* |

### 3. Ownership check
Added `RecordedByStaffProfileId` (nullable `int`) to the `VitalSign`
entity, populated from the `staffProfileId` JWT claim at creation time
(`VitalSignsController.Create` → `VitalSignService.RecordReadingAsync`).

`GET /api/v1/VitalSigns/{id}` enforces ownership: a Nurse may only view a
reading she personally recorded; Doctor, Auditor, and Admin bypass the
check since they need visibility across all patients.

```csharp
var isPrivileged = User.IsInRole("Doctor") || User.IsInRole("Auditor") || User.IsInRole("Admin");
if (!isPrivileged && vital.RecordedByStaffProfileId != CurrentStaffProfileId)
    return Forbid();
```

### 4 & 5. Tested via Postman
- Registered three Nurse accounts, promoted one to Doctor via the seeded
  Admin account.
- Confirmed a Nurse token receives `403 Forbidden` from two Doctor/Admin-only
  endpoints: `GET /VitalSigns` (all) and `GET /VitalSigns/critical`.
- Confirmed a Doctor token succeeds (`200`) on both of the same endpoints.
- Confirmed one Nurse's token cannot access a specific VitalSign reading
  recorded by a different Nurse (`403`), while the recording Nurse and any
  Doctor/Admin can (`200`).

See `postman/` for the exported collection with all Day 3 requests.

## Real Issues Found and Fixed Along the Way

1. **Migration for the new `RecordedByStaffProfileId` column** was
   reviewed before applying — confirmed as a single, safe nullable
   `AddColumn` with no unintended drops.

2. **Test suite regression after the Contract change**: `RecordReadingAsync`
   gained a new required parameter (`recordedByStaffProfileId`), which
   broke `VitalSignServiceTests.cs`'s three existing calls (compiler
   errors) — fixed by updating every call site and adding a fourth test
   (`RecordReadingAsync_PersistsRecordedByStaffProfileId`) to actually
   verify the new ownership link is written to the database, not just
   that the code compiles.

3. **Integration test regressions**, found by running the full suite
   after the RBAC changes (24 → 26 tests, several rewritten):
   - `CardiacApiFactory.RegisterAndLoginAsync` and two test classes'
     private register helpers still POSTed only `{ email, password }` —
     missing the `Department` field `RegisterRequest` now requires since
     Day 2. Fixed by adding `department` to every register call.
   - `RoleBasedAccessTests` needed a way to promote a freshly-registered
     account to Doctor without `assign-role` being open to anyone
     anymore — solved by logging in as the seeded Admin account first,
     purely as a test fixture step.
   - Running several logins in quick succession (register → promote →
     log in again, repeated across multiple RBAC tests) tripped the real
     `"login"` rate limiter (5 requests/minute) in the test environment,
     producing a `429` with an empty body that initially surfaced as a
     confusing `KeyNotFoundException`. Fixed properly by having
     `CardiacApiFactory` replace the rate limiter's *options* with an
     effectively-unlimited version for tests only — the real 5/minute
     policy in `Program.cs` itself is completely untouched.
   - `VitalSignsApiTests.GetAll_ReturnsSuccess_WhenRequestCarriesAValidJwt`
     was testing a rule that no longer exists (a plain authenticated user
     could see all vitals) — rewritten into two tests that match the new
     rule: one confirming a privileged (Admin) token succeeds, one
     confirming a plain Nurse token is correctly rejected with `403`.

4. **First attempt at replacing the rate limiter** used
   `services.Configure<RateLimiterOptions>()` directly on top of the
   existing registration, which threw `"There already exists a policy
   with the name login"` — `AddFixedWindowLimiter` doesn't allow
   redefining a policy name that's already configured. Fixed by first
   removing the original `RateLimiterOptions` service descriptor, then
   configuring fresh policies with the same names.

## Test Suite Status
**26/26 tests passing** (`dotnet test`), up from 24 at the end of Day 2 —
two new integration tests were added specifically for the new RBAC rule
on `VitalSigns/GetAll`.

## Key Takeaway
Every time an endpoint's authorization rule changes, any integration test
asserting the *old* rule isn't just outdated — it becomes actively wrong
and needs to be rewritten to assert the new rule, not deleted or ignored.
Running the full test suite immediately after an RBAC change is what
surfaces this; skipping it would have shipped a regression silently.
