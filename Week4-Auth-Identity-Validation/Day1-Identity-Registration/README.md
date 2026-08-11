# Day 1 — ASP.NET Core Identity & User Registration

**8 hours**

## Learning Objectives

- Explain what ASP.NET Core Identity provides out of the box
- Set up Identity with Entity Framework Core
- Implement a user registration endpoint

## What I Did

- Installed `Microsoft.AspNetCore.Identity.EntityFrameworkCore` and
  extended `AppDbContext` to inherit from `IdentityDbContext<IdentityUser>`,
  bringing in the full Identity schema (`AspNetUsers`, `AspNetRoles`,
  and supporting tables) alongside the Week 3 entities
- Migrated the Week 3 domain model to work with Identity: replaced
  the custom `User` entity with `IdentityUser`, and changed
  `Project.OwnerId` and `TaskItem.AssignedToUserId` from `int` to
  `string` to match `IdentityUser.Id`'s type
- Registered Identity services in `Program.cs` with
  `AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<AppDbContext>()`
- Ran a migration (`InitialCreateWithIdentity`) creating both the
  Identity tables and the app's own tables in one step
- Implemented `POST /api/v1/auth/register` using
  `UserManager<IdentityUser>.CreateAsync`, returning the specific
  validation errors from Identity's `IdentityResult` instead of a
  generic failure message

## Postman Collection — Testing & Documentation

Following mentor feedback on Week 3 ("the Postman testing should be
clearer"), this week's collection raises the documentation bar:

- **Collection-level documentation**: overview, base URL, auth model,
  folder structure, and run instructions, visible before opening any
  individual request
- **Folder-level documentation**: what each folder covers and why
- **Per-request documentation**: purpose, expected response (with a
  sample body), and any relevant notes — written *before* running the
  request, not just inferred from the response after the fact
- **Descriptive, numbered request names** (`01 - Register (Valid) → 200`)
  so the expected outcome is visible from the request list alone
- **Saved response examples** for both the success and error cases,
  so the collection is understandable without re-running every request
- **3 requests covering registration**: a valid registration (200), a
  weak password (400, with Identity's specific policy violations),
  and a duplicate email (400) — not just the happy path

Exported to
[`TaskTrackerApi-Postman-Collection-Week4.json`](C:\Projects\BinX-Backend-Internship\Week4-Auth-Identity-Validation\Day1-Identity-Registration\TaskTrackerApi-Postman-Collection-Week4.json).

## Key Code Example

```csharp
[HttpPost("register")]
public async Task<IActionResult> Register(RegisterRequest request)
{
    var user = new IdentityUser { UserName = request.Email, Email = request.Email };

    var result = await _userManager.CreateAsync(user, request.Password);

    if (!result.Succeeded)
    {
        var errors = result.Errors.Select(e => e.Description);
        return BadRequest(new { errors });
    }

    return Ok(new { message = "User registered successfully.", userId = user.Id });
}
```

## What I Learned

Migrating an existing domain model to work with Identity was more
involved than expected — `IdentityUser.Id` is a `string` (a GUID),
not an `int`, so every foreign key referencing a user (`Project.OwnerId`,
`TaskItem.AssignedToUserId`) had to change type, which cascaded into
compiler errors in the `DbContext` and controller until every
reference was updated consistently.

Testing the duplicate-email case surfaced an ordering issue in the
Postman collection itself: reusing an email across requests without
tracking what had already been registered caused a "should succeed"
request to fail with a duplicate-email error, since the database
persists between test runs unlike a fresh unit test. This reinforced
why the Week 3 feedback on clearer Postman testing mattered — a
request's *expected* outcome needs to be documented and verified
deliberately, not assumed from what happened to run successfully once.

Identity's default password policy (minimum length, uppercase,
lowercase, non-alphanumeric character) also caught "123" with four
distinct, specific error messages without any custom validation code
being written — confirming that Identity's battle-tested defaults
catch more than a hand-rolled check would.

## Project

[`TaskTrackerApi/`](TaskTrackerApi/) — Week 3's CRUD API extended
with ASP.NET Core Identity and a working registration endpoint.