# Day 4 — Input Validation with FluentValidation

### Week 4 · BinX Backend Development Internship (.NET)

---

## 1. Learning Objectives

- Compare DataAnnotations and FluentValidation, and choose FluentValidation for
  business-rule validation.
- Write validators expressing real business rules, not just "field is not null".
- Integrate validation into the request pipeline and return structured, specific
  error responses.
- Test each validation rule individually and confirm the exact error message returned.

## 2. What I Built

- Installed `FluentValidation.AspNetCore`.
- Created `Validators/CreateTaskValidator.cs` with **3 real business rules**:
  1. `Title` must be present and no longer than 200 characters.
  2. `PriorityLevel` must fall between 1 and 5 (not just "not null" — an out-of-range
     int like `99` is a valid integer but a meaningless priority in this domain).
  3. `ProjectId` must reference a project that **actually exists** in the database —
     checked with an async EF Core query (`MustAsync`), not just a range/type check.
- Created `Validators/UpdateTaskValidator.cs` reusing the Title and PriorityLevel rules.
- Registered both validators in `Program.cs` via `AddValidatorsFromAssemblyContaining<Program>()`.
- Injected `IValidator<CreateTaskRequest>` / `IValidator<UpdateTaskRequest>` directly into
  `TasksController` and call `ValidateAsync(...)` explicitly inside `Create` and `Update`,
  returning `400 BadRequest` with the specific list of failed rules when invalid.

## 3. Key Challenge: Automatic Validation Doesn't Support Async Rules

**The problem:** FluentValidation's ASP.NET Core integration offers an "automatic"
mode (`AddFluentValidationAutoValidation()`) that runs validators as part of MVC's
model-binding pipeline, before the controller action executes. It looked like the
obvious choice — until every request, valid or not, sailed straight through to the
database and only failed on the SQL `FOREIGN KEY` constraint instead of returning a
clean `400`.

**Root cause:** ASP.NET Core's model-binding validation pipeline is **synchronous**.
`CreateTaskValidator`'s `ProjectId` rule uses `MustAsync` to query the database — a
genuinely asynchronous rule. Automatic validation throws
`AsyncValidatorInvokedSynchronouslyException` the moment it hits an async rule, and
(in this ASP.NET Core version) that exception surfaced as an unhandled `500` rather
than blocking cleanly, which is why the validator appeared to do nothing at all.

**The fix:** Removed `AddFluentValidationAutoValidation()` entirely. Kept only
`AddValidatorsFromAssemblyContaining<Program>()` (registers the validators with DI,
nothing more), and call `ValidateAsync()` **explicitly** inside each controller action:

```csharp
var validationResult = await _createValidator.ValidateAsync(request);
if (!validationResult.IsValid)
{
    return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
}
```

This keeps the async chain intact end-to-end (Week 2's "async all the way" rule) and
lets `ProjectId`'s database check run exactly like every other async EF Core query in
the project.

**A second, unrelated bug found along the way:** `TasksController.cs` had its own
local `public record CreateTaskRequest(...)` declared directly above the class,
defined in the same namespace as the controller. Because of C# namespace resolution,
the controller was binding to *that* local record instead of the one in
`Models/CreateTaskRequest.cs` — the one the validator was actually registered against.
Two different types, same name, only one had a validator wired to it. Removing the
duplicate declaration fixed the mismatch.

## 4. Verification Results (Postman / Swagger)

All requests below were sent with a valid JWT (`Authorization: Bearer <token>`),
confirming validation runs *after* authentication, as expected.

| # | Request | Body (abridged) | Result |
|---|---|---|---|
| 1 | `POST /api/v1/Tasks` | `title: "  "`, `priorityLevel: 0`, `projectId: 0` | `400` → `["Title is required.", "PriorityLevel must be between 1 (lowest) and 5 (highest).", "The specified ProjectId does not exist."]` |
| 2 | `POST /api/v1/Tasks` | `title: "string"`, `priorityLevel: 99`, `projectId: 0` | `400` → `["PriorityLevel must be between 1 (lowest) and 5 (highest).", "The specified ProjectId does not exist."]` (Title valid → no Title error) |
| 3 | `POST /api/v1/Tasks` | `title: "string"`, `priorityLevel: 0`, `projectId: 9999` | `400` → `["PriorityLevel must be between 1 (lowest) and 5 (highest).", "The specified ProjectId does not exist."]` |
| 4 | `POST /api/v1/Tasks` | all fields valid, real `projectId` | `201 Created` — task persisted with a real `Id` |
| 5 | `PUT /api/v1/Tasks/{id}` | `title: "  "`, `priorityLevel: 10`, `isCompleted: false` | `400` — confirmed after fixing request Method (was POST, corrected to PUT) and URL (added missing `/{id}` segment) |

Each error list contains **only** the rules that actually failed for that specific
request — confirming the validators are independent and accurate, not one hardcoded
message reused for every failure (which was the earlier, broken behavior).

## 5. A Seeding Note

The database started with no `Project` rows, so `ProjectId` existence couldn't be
tested against a real record. A temporary seed block was added to `Program.cs`
(creates one `Project` on startup if the table is empty, owned by the first
registered `IdentityUser`) purely to enable end-to-end testing of Rule 3. This is
throwaway test data, not part of the permanent seed strategy, and is safe to remove
once verification is complete.

## 6. How to Test (Postman)

1. Import `postman/Day4-FluentValidation.postman_collection.json` from this folder.
2. Run **Register** → **Login** to capture a JWT into the environment automatically.
3. Run each request in the **Invalid Requests** folder individually — confirm each
   returns `400` with the specific field message(s) expected.
4. Run **Create Task (Valid)** — confirm `201 Created` with a persisted task.
5. Run **Update Task (Invalid)** and **Update Task (Valid)** the same way against the
   task Id returned in step 4.

## 7. Key Takeaways

- FluentValidation's *automatic* pipeline mode is synchronous-only — any validator
  with an async rule (database lookups, external calls) must be invoked manually
  with `ValidateAsync()` inside the action instead.
- A duplicate type name in the same namespace silently shadows the "real" one — the
  compiler picks the nearest matching type with no warning, which made this bug
  much harder to spot than a compile error would have been.
- Structured error responses (`Errors.Select(e => e.ErrorMessage)`) instead of one
  generic string let a client immediately see exactly which fields failed and why.

---

**Status:** Complete. Create and Update validated end-to-end via Swagger and Postman,
including error paths (Title, PriorityLevel, ProjectId existence) and the happy path
(`201 Created`). Postman collection with automated status-code assertions exported
and included in this folder.
