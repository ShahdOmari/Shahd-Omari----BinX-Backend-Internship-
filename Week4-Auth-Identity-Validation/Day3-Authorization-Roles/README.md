# Day 3 — Authorization & Role-Based Access Control (RBAC)

### Week 4 · BinX Backend Development Internship (.NET)

---

## 1. Learning Objectives

- Apply the `[Authorize]` attribute to protect endpoints from unauthenticated access.
- Implement role-based access control with at least two roles (Admin, User).
- Understand and apply claims-based / policy-based authorization for finer-grained control.
- Test protected routes end-to-end using Postman, including token capture and reuse.

## 2. What I Built

- Protected `TasksController` entirely with `[Authorize]` — any request without a valid
  JWT now returns `401 Unauthorized` before reaching controller logic.
- Created two roles, **Admin** and **User**, using `RoleManager<IdentityRole>`.
- Restricted the `DELETE` endpoint to the **Admin** role only via
  `[Authorize(Roles = "Admin")]`.
- Added the user's assigned roles as claims inside the JWT itself at login time — required
  for `[Authorize(Roles = ...)]` to work at all, since role checks read directly from the
  token's claims, not from the database on every request.
- Defined a named authorization policy, `CanManageProjects`, combining more than a single
  role check, and applied it to a project-management endpoint.

## 3. Verification

| Scenario | Expected | Result |
|---|---|---|
| `DELETE` task with Admin token | Passes authorization, reaches lookup logic | ✅ (`404` — task didn't exist, but authorization passed) |
| `DELETE` task with User token | `403 Forbidden` (authenticated, not permitted) | ✅ |
| Any `TasksController` request with no token | `401 Unauthorized` | ✅ |
| `CanManageProjects` policy on a project endpoint | Enforces combined claim conditions | ✅ |

## 4. Challenges & Fixes

- **`405 Method Not Allowed` on Register User (Postman Runner)** — traced to the request's
  HTTP method / URL configuration in the collection, not an API bug; being verified request
  by request outside the Runner.
- **`401` instead of expected `403` on Delete Task (User)** — caused by an upstream failed
  Register/Login step in the Runner sequence, meaning no token was ever attached to the
  Delete request. Fix: use fresh, never-used emails per Runner run, and verify each step's
  token is actually captured into the environment before the next request depends on it.
- **General lesson**: a Postman Runner failure early in a sequence (e.g. a `400` on
  Register because the email already exists) can cascade into misleading status codes
  further down the chain — always check the first failure in the sequence first.

## 5. How to Test

1. Import the Postman collection from `postman/Day3-Authorization.postman_collection.json`
   in this folder.
2. Set the `{{baseUrl}}` environment variable to your local API URL.
3. Run **Register Admin** → **Assign Admin Role** → **Login as Admin** to capture the admin
   token automatically.
4. Run **Register User** → **Login as User** to capture the user token.
5. Run the protected `DELETE` request twice — once with the Admin token (should pass
   authorization) and once with the User token (should return `403`).

## 6. Key Takeaways

- `[Authorize]` alone only checks *authentication* (who you are); `[Authorize(Roles = ...)]`
  and policies check *authorization* (what you're allowed to do) — a `403` means the first
  check passed and the second one failed.
- Roles must be embedded as claims in the JWT at login time — assigning a role in the
  database alone is not enough for `[Authorize(Roles = ...)]` to see it.
- Policies are the more maintainable choice once an authorization rule needs more than a
  single role check, since the rule lives in one place instead of being repeated as
  attribute strings across controllers.

---

**Status:** Code complete and manually verified; Postman Runner sequence being finalized
before push.