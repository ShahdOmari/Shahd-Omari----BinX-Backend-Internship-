# Day 5 — Securing the API: Rate Limiting, CORS & Security Headers

### Week 4 · BinX Backend Development Internship (.NET)

---

## 1. Learning Objectives

- Configure rate limiting to protect against brute-force and denial-of-service patterns.
- Configure CORS correctly for the API's real intended consumers.
- Apply security headers (HTTPS redirection, HSTS) and confirm EF Core prevents
  SQL injection by default.

## 2. What I Built

- **Rate limiting** (`AddRateLimiter` with two named fixed-window limiters):
  - `"general"` — 100 requests per minute, applied to `TasksController` as a whole.
  - `"login"` — 5 requests per minute, applied only to `POST /api/v1/Auth/login`,
    since repeated rapid login attempts are the clearest sign of a brute-force
    attack in progress.
  - Exceeding a limit returns `429 Too Many Requests` (`RejectionStatusCode`).
- **CORS** — a single named policy, `AllowFrontend`, allowing only
  `https://tasktracker-frontend.com` with any header/method. Deliberately *not* a
  permissive "allow any origin" policy, since that would let any website's script
  call this API on a logged-in user's behalf.
- **HTTPS / HSTS** — `app.UseHttpsRedirection()` runs unconditionally;
  `app.UseHsts()` runs only outside `Development`, since HSTS would otherwise block
  local HTTP testing in the browser.
- **Pipeline ordering** — CORS and rate limiting are registered *before*
  authentication/authorization, so a disallowed origin or a rate-limited client is
  rejected before the request ever reaches an identity check:
  ```csharp
  app.UseHttpsRedirection();
  app.UseCors("AllowFrontend");
  app.UseRateLimiter();
  app.UseAuthentication();
  app.UseAuthorization();
  ```
- **SQL injection review** — searched the entire codebase for `FromSqlRaw`,
  `ExecuteSqlRaw`, and `FromSqlInterpolated`. None found — every database query in
  the project goes through EF Core's LINQ methods, which parameterize values
  automatically. There is no place in this codebase where user input is
  concatenated directly into a raw SQL string.

## 3. Verification Results

### Rate Limiting

Sent 6 rapid, sequential requests to `POST /api/v1/Auth/login` with invalid
credentials via a PowerShell loop:

| Attempt | Status | Notes |
|---|---|---|
| 1 | `401` | Invalid credentials — expected, request still processed normally |
| 2 | `401` | " |
| 3 | `401` | " |
| 4 | `401` | " |
| 5 | `401` | " |
| 6 | **`429`** | **Rate limit exceeded** — rejected before reaching the login logic |

Confirms the `"login"` limiter's 5-requests-per-minute cap is enforced correctly,
independent of whether the credentials themselves are valid.

### CORS

Created a standalone HTML file served from `file://` (an origin entirely different
from the allowed `https://tasktracker-frontend.com`) that attempted a `fetch()` call
to `GET /api/v1/Tasks`. Result:

```
BLOCKED BY CORS: Failed to fetch
```

Confirms the browser's CORS enforcement rejects the request before it can read the
response, exactly as expected for a disallowed origin. (Note: CORS is a
browser-enforced restriction on JavaScript — it does not block direct HTTP clients
like Postman, curl, or Swagger, which is why this had to be tested from an actual
browser context rather than from the API testing tools used in Days 1–4.)

### HTTPS Redirection & HSTS

Configured and present in the middleware pipeline; HSTS is conditionally skipped in
`Development` by design (browsers would otherwise block local `http://localhost`
testing entirely).

### SQL Injection Review

```powershell
Get-ChildItem -Path . -Filter "*.cs" -Recurse | Select-String -Pattern "FromSqlRaw|ExecuteSqlRaw|FromSqlInterpolated"
```
No matches found across the entire project.

## 4. Challenges & Fixes

- **PowerShell version differences**: `Invoke-WebRequest -SkipHttpErrorCheck` is
  only available in PowerShell 7+; on PowerShell 5.1 (the Windows default), a
  non-2xx response throws an exception instead of returning a response object with
  a status code. Fixed by wrapping the request in `try/catch` and reading
  `$_.Exception.Response.StatusCode.value__` in the catch block.
- **`$_` inside a string**: `"Attempt $_: Status ..."` fails to parse because
  PowerShell interprets `$_:` as a drive-qualified variable reference. Fixed by
  assigning `$_` to a named variable (`$i`) before interpolating it into the string.

## 5. How to Test (Postman)

1. Import `postman/Day5-Hardening.postman_collection.json` from this folder.
2. Run **Login** 6 times in quick succession (or use the collection's built-in
   Runner with no delay) — confirm the 6th request returns `429`.
3. CORS cannot be meaningfully tested from Postman itself (see note above); refer to
   `cors-test.html` included in this folder, or open it directly in a browser
   against a running instance of the API.

## 6. Key Takeaways

- Rate limiting, CORS, and HSTS are a handful of lines each, but they close off
  entire categories of real-world attack (brute-force credential stuffing,
  cross-origin credential misuse, and downgrade-to-HTTP attacks respectively) —
  skipping them is a common but costly shortcut.
- CORS protects browsers, not APIs directly — a disallowed origin only matters to
  JavaScript running in a browser; it does not stop a direct request from a script,
  curl, or a tool like Postman. Testing it requires an actual browser context.
- EF Core's LINQ-based query methods parameterize every value by default; SQL
  injection risk in an EF Core project is really a risk introduced only by
  deliberately dropping down to raw, string-interpolated SQL — something this
  project never does.

---

**Status:** Complete. All four hardening requirements implemented and verified:
rate limiting (differentiated login vs. general limits), CORS (rejecting a
disallowed origin), HTTPS/HSTS, and a clean SQL-injection review. This closes out
Week 4.